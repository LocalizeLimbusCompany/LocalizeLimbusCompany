using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LimbusLocalize_Updater;

public enum JsonNodeType
{
	Array = 1,
	Object = 2,
	String = 3,
	Number = 4,
	NullValue = 5,
	Boolean = 6,
	None = 7,
	Custom = 0xFF
}

public enum JsonTextMode
{
	Compact,
	Indent
}

public abstract class JsonNode
{
	[ThreadStatic] private static StringBuilder _mEscapeBuilder;

	internal static StringBuilder EscapeBuilder => _mEscapeBuilder ??= new StringBuilder();

	internal static string Escape(string aText)
	{
		var sb = EscapeBuilder;
		sb.Length = 0;
		if (sb.Capacity < aText.Length + aText.Length / 10)
			sb.Capacity = aText.Length + aText.Length / 10;
		foreach (var c in aText)
			switch (c)
			{
				case '\\':
					sb.Append(@"\\");
					break;
				case '\"':
					sb.Append("\\\"");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				case '\b':
					sb.Append("\\b");
					break;
				case '\f':
					sb.Append("\\f");
					break;
				default:
					if (c < ' ' || (ForceAscii && c > 127))
					{
						ushort val = c;
						sb.Append("\\u").Append(val.ToString("X4"));
					}
					else
					{
						sb.Append(c);
					}

					break;
			}

		var result = sb.ToString();
		sb.Length = 0;
		return result;
	}

	private static JsonNode ParseElement(string token, bool quoted)
	{
		if (quoted)
			return token;
		if (token.Length <= 5)
		{
			var tmp = token.ToLower();
			switch (tmp)
			{
				case "false":
				case "true":
					return tmp == "true";
				case "null":
					return JsonNull.CreateOrGet();
			}
		}

		if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
			return val;
		return token;
	}

	public static JsonNode Parse(string aJSON)
	{
		Stack<JsonNode> stack = new();
		JsonNode ctx = null;
		var i = 0;
		StringBuilder token = new();
		var tokenName = "";
		var quoteMode = false;
		var tokenIsQuoted = false;
		var hasNewlineChar = false;
		while (i < aJSON.Length)
		{
			switch (aJSON[i])
			{
				case '{':
					if (quoteMode)
					{
						token.Append(aJSON[i]);
						break;
					}

					stack.Push(new JsonObject());
					ctx?.Add(tokenName, stack.Peek());
					tokenName = "";
					token.Length = 0;
					ctx = stack.Peek();
					hasNewlineChar = false;
					break;

				case '[':
					if (quoteMode)
					{
						token.Append(aJSON[i]);
						break;
					}

					stack.Push(new JsonArray());
					ctx?.Add(tokenName, stack.Peek());
					tokenName = "";
					token.Length = 0;
					ctx = stack.Peek();
					hasNewlineChar = false;
					break;

				case '}':
				case ']':
					if (quoteMode)
					{
						token.Append(aJSON[i]);
						break;
					}

					if (stack.Count == 0)
						throw new Exception("JSON Parse: Too many closing brackets");

					stack.Pop();
					if (token.Length > 0 || tokenIsQuoted)
						ctx.Add(tokenName, ParseElement(token.ToString(), tokenIsQuoted));
					if (ctx != null)
						ctx.Inline = !hasNewlineChar;
					tokenIsQuoted = false;
					tokenName = "";
					token.Length = 0;
					if (stack.Count > 0)
						ctx = stack.Peek();
					break;

				case ':':
					if (quoteMode)
					{
						token.Append(aJSON[i]);
						break;
					}

					tokenName = token.ToString();
					token.Length = 0;
					tokenIsQuoted = false;
					break;

				case '"':
					quoteMode ^= true;
					tokenIsQuoted |= quoteMode;
					break;

				case ',':
					if (quoteMode)
					{
						token.Append(aJSON[i]);
						break;
					}

					if (token.Length > 0 || tokenIsQuoted)
						ctx.Add(tokenName, ParseElement(token.ToString(), tokenIsQuoted));
					tokenName = "";
					token.Length = 0;
					tokenIsQuoted = false;
					break;

				case '\r':
				case '\n':
					hasNewlineChar = true;
					break;

				case ' ':
				case '\t':
					if (quoteMode)
						token.Append(aJSON[i]);
					break;

				case '\\':
					++i;
					if (quoteMode)
					{
						var c = aJSON[i];
						switch (c)
						{
							case 't':
								token.Append('\t');
								break;
							case 'r':
								token.Append('\r');
								break;
							case 'n':
								token.Append('\n');
								break;
							case 'b':
								token.Append('\b');
								break;
							case 'f':
								token.Append('\f');
								break;
							case 'u':
							{
								var s = aJSON.Substring(i + 1, 4);
								token.Append((char)int.Parse(
									s,
									NumberStyles.AllowHexSpecifier));
								i += 4;
								break;
							}
							default:
								token.Append(c);
								break;
						}
					}

					break;
				case '/':
					if (AllowLineComments && !quoteMode && i + 1 < aJSON.Length && aJSON[i + 1] == '/')
					{
						while (++i < aJSON.Length && aJSON[i] != '\n' && aJSON[i] != '\r') ;
						break;
					}

					token.Append(aJSON[i]);
					break;
				case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
					break;

				default:
					token.Append(aJSON[i]);
					break;
			}

			++i;
		}

		if (quoteMode) throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
		return ctx == null ? ParseElement(token.ToString(), tokenIsQuoted) : ctx;
	}

	#region Enumerators

	public struct Enumerator
	{
		private enum Type
		{
			None,
			Array,
			Object
		}

		private readonly Type _type;
		private Dictionary<string, JsonNode>.Enumerator _mObject;
		private List<JsonNode>.Enumerator _mArray;
		public bool IsValid => _type != Type.None;

		public Enumerator(List<JsonNode>.Enumerator aArrayEnum)
		{
			_type = Type.Array;
			_mObject = default;
			_mArray = aArrayEnum;
		}

		public Enumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum)
		{
			_type = Type.Object;
			_mObject = aDictEnum;
			_mArray = default;
		}

		public KeyValuePair<string, JsonNode> Current
		{
			get
			{
				return _type switch
				{
					Type.Array => new KeyValuePair<string, JsonNode>(string.Empty, _mArray.Current),
					Type.Object => _mObject.Current,
					_ => new KeyValuePair<string, JsonNode>(string.Empty, null)
				};
			}
		}

		public bool MoveNext()
		{
			return _type switch
			{
				Type.Array => _mArray.MoveNext(),
				Type.Object => _mObject.MoveNext(),
				_ => false
			};
		}
	}

	public struct ValueEnumerator(Enumerator aEnumerator)
	{
		private Enumerator _mEnumerator = aEnumerator;

		public ValueEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum))
		{
		}

		public ValueEnumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum))
		{
		}

		public JsonNode Current => _mEnumerator.Current.Value;

		public bool MoveNext()
		{
			return _mEnumerator.MoveNext();
		}

		public ValueEnumerator GetEnumerator()
		{
			return this;
		}
	}

	public struct KeyEnumerator(Enumerator aEnumerator)
	{
		private Enumerator _mEnumerator = aEnumerator;

		public KeyEnumerator(List<JsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum))
		{
		}

		public KeyEnumerator(Dictionary<string, JsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum))
		{
		}

		public string Current => _mEnumerator.Current.Key;

		public bool MoveNext()
		{
			return _mEnumerator.MoveNext();
		}

		public KeyEnumerator GetEnumerator()
		{
			return this;
		}
	}

	public class LinqEnumerator : IEnumerator<KeyValuePair<string, JsonNode>>,
		IEnumerable<KeyValuePair<string, JsonNode>>
	{
		private Enumerator _mEnumerator;
		private JsonNode _mNode;

		internal LinqEnumerator(JsonNode aNode)
		{
			_mNode = aNode;
			if (_mNode != null)
				_mEnumerator = _mNode.GetEnumerator();
		}

		public IEnumerator<KeyValuePair<string, JsonNode>> GetEnumerator()
		{
			return new LinqEnumerator(_mNode);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new LinqEnumerator(_mNode);
		}

		public KeyValuePair<string, JsonNode> Current => _mEnumerator.Current;
		object IEnumerator.Current => _mEnumerator.Current;

		public bool MoveNext()
		{
			return _mEnumerator.MoveNext();
		}

		public void Dispose()
		{
			_mNode = null;
			_mEnumerator = new Enumerator();
		}

		public void Reset()
		{
			if (_mNode != null)
				_mEnumerator = _mNode.GetEnumerator();
		}
	}

	#endregion Enumerators

	#region common interface

	public static bool ForceAscii = false; // Use Unicode by default
	public static bool LongAsString = false; // lazy creator creates a JSONString instead of JSONNumber
	public static bool AllowLineComments = true; // allow "//"-style comments at the end of a line

	public abstract JsonNodeType Tag { get; }

	public virtual JsonNode this[int aIndex]
	{
		get => null;
		set { }
	}

	public virtual JsonNode this[string aKey]
	{
		get => null;
		set { }
	}

	public virtual string Value
	{
		get => "";
		set { }
	}

	public virtual int Count => 0;

	public virtual bool IsNumber => false;
	public virtual bool IsString => false;
	public virtual bool IsBoolean => false;
	public virtual bool IsNull => false;
	public virtual bool IsArray => false;
	public virtual bool IsObject => false;

	public virtual bool Inline
	{
		get => false;
		set { }
	}

	public virtual void Add(string aKey, JsonNode aItem)
	{
	}

	public virtual void Add(JsonNode aItem)
	{
		Add("", aItem);
	}

	public virtual JsonNode Remove(string aKey)
	{
		return null;
	}

	public virtual JsonNode Remove(int aIndex)
	{
		return null;
	}

	public virtual JsonNode Remove(JsonNode aNode)
	{
		return aNode;
	}

	public virtual void Clear()
	{
	}

	public virtual JsonNode Clone()
	{
		return null;
	}

	public virtual IEnumerable<JsonNode> Children
	{
		get { yield break; }
	}

	public IEnumerable<JsonNode> DeepChildren
	{
		get { return Children.SelectMany(c => c.DeepChildren); }
	}

	public virtual bool HasKey(string aKey)
	{
		return false;
	}

	public virtual JsonNode GetValueOrDefault(string aKey, JsonNode aDefault)
	{
		return aDefault;
	}

	public override string ToString()
	{
		StringBuilder sb = new();
		WriteToStringBuilder(sb, 0, 0, JsonTextMode.Compact);
		return sb.ToString();
	}

	public virtual string ToString(int aIndent)
	{
		StringBuilder sb = new();
		WriteToStringBuilder(sb, 0, aIndent, JsonTextMode.Indent);
		return sb.ToString();
	}

	internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode);

	public abstract Enumerator GetEnumerator();
	public IEnumerable<KeyValuePair<string, JsonNode>> Linq => new LinqEnumerator(this);
	public KeyEnumerator Keys => new(GetEnumerator());
	public ValueEnumerator Values => new(GetEnumerator());

	#endregion common interface

	#region typecasting properties

	public virtual double AsDouble
	{
		get => double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0.0;
		set => Value = value.ToString(CultureInfo.InvariantCulture);
	}

	public virtual int AsInt
	{
		get => (int)AsDouble;
		set => AsDouble = value;
	}

	public virtual float AsFloat
	{
		get => (float)AsDouble;
		set => AsDouble = value;
	}

	public virtual bool AsBool
	{
		get
		{
			if (bool.TryParse(Value, out var v))
				return v;
			return !string.IsNullOrEmpty(Value);
		}
		set => Value = value ? "true" : "false";
	}

	public virtual long AsLong
	{
		get => long.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : 0L;
		set => Value = value.ToString(CultureInfo.InvariantCulture);
	}

	public virtual ulong AsULong
	{
		get => ulong.TryParse(Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : 0;
		set => Value = value.ToString(CultureInfo.InvariantCulture);
	}

	public virtual JsonArray AsArray => this as JsonArray;

	public virtual JsonObject AsObject => this as JsonObject;

	#endregion typecasting properties

	#region operators

	public static implicit operator JsonNode(string s)
	{
		return s == null ? JsonNull.CreateOrGet() : new JsonString(s);
	}

	public static implicit operator string(JsonNode d)
	{
		return d?.Value;
	}

	public static implicit operator JsonNode(double n)
	{
		return new JsonNumber(n);
	}

	public static implicit operator double(JsonNode d)
	{
		return d == null ? 0 : d.AsDouble;
	}

	public static implicit operator JsonNode(float n)
	{
		return new JsonNumber(n);
	}

	public static implicit operator float(JsonNode d)
	{
		return d == null ? 0 : d.AsFloat;
	}

	public static implicit operator JsonNode(int n)
	{
		return new JsonNumber(n);
	}

	public static implicit operator int(JsonNode d)
	{
		return d == null ? 0 : d.AsInt;
	}

	public static implicit operator JsonNode(long n)
	{
		if (LongAsString)
			return new JsonString(n.ToString(CultureInfo.InvariantCulture));
		return new JsonNumber(n);
	}

	public static implicit operator long(JsonNode d)
	{
		return d == null ? 0L : d.AsLong;
	}

	public static implicit operator JsonNode(ulong n)
	{
		if (LongAsString)
			return new JsonString(n.ToString(CultureInfo.InvariantCulture));
		return new JsonNumber(n);
	}

	public static implicit operator ulong(JsonNode d)
	{
		return d == null ? 0 : d.AsULong;
	}

	public static implicit operator JsonNode(bool b)
	{
		return new JsonBool(b);
	}

	public static implicit operator bool(JsonNode d)
	{
		return d != null && d.AsBool;
	}

	public static implicit operator JsonNode(KeyValuePair<string, JsonNode> aKeyValue)
	{
		return aKeyValue.Value;
	}

	public static bool operator ==(JsonNode a, object b)
	{
		if (ReferenceEquals(a, b))
			return true;
		var aIsNull = a is JsonNull or null or JsonLazyCreator;
		var bIsNull = b is JsonNull or null or JsonLazyCreator;
		if (aIsNull && bIsNull)
			return true;
		return !aIsNull && a.Equals(b);
	}

	public static bool operator !=(JsonNode a, object b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		return ReferenceEquals(this, obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	#endregion operators
}
// End of JSONNode

public class JsonArray : JsonNode
{
	private bool _inline;
	public List<JsonNode> List { get; } = [];

	public override bool Inline
	{
		get => _inline;
		set => _inline = value;
	}

	public override JsonNodeType Tag => JsonNodeType.Array;
	public override bool IsArray => true;

	public override JsonNode this[int aIndex]
	{
		get
		{
			if (aIndex < 0 || aIndex >= List.Count)
				return new JsonLazyCreator(this);
			return List[aIndex];
		}
		set
		{
			if (value == null)
				value = JsonNull.CreateOrGet();
			if (aIndex < 0 || aIndex >= List.Count)
				List.Add(value);
			else
				List[aIndex] = value;
		}
	}

	public override JsonNode this[string aKey]
	{
		get => new JsonLazyCreator(this);
		set
		{
			if (value == null)
				value = JsonNull.CreateOrGet();
			List.Add(value);
		}
	}

	public override int Count => List.Count;

	public override IEnumerable<JsonNode> Children => List;

	public override Enumerator GetEnumerator()
	{
		return new Enumerator(List.GetEnumerator());
	}

	public override void Add(string aKey, JsonNode aItem)
	{
		if (aItem == null)
			aItem = JsonNull.CreateOrGet();
		List.Add(aItem);
	}

	public override JsonNode Remove(int aIndex)
	{
		if (aIndex < 0 || aIndex >= List.Count)
			return null;
		var tmp = List[aIndex];
		List.RemoveAt(aIndex);
		return tmp;
	}

	public override JsonNode Remove(JsonNode aNode)
	{
		List.Remove(aNode);
		return aNode;
	}

	public override void Clear()
	{
		List.Clear();
	}

	public override JsonNode Clone()
	{
		var node = new JsonArray
		{
			List =
			{
				Capacity = List.Capacity
			}
		};
		foreach (var n in List) node.Add(n != null ? n.Clone() : null);
		return node;
	}


	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
	{
		aSB.Append('[');
		var count = List.Count;
		if (_inline)
			aMode = JsonTextMode.Compact;
		for (var i = 0; i < count; i++)
		{
			if (i > 0)
				aSB.Append(',');
			if (aMode == JsonTextMode.Indent)
				aSB.AppendLine();

			if (aMode == JsonTextMode.Indent)
				aSB.Append(' ', aIndent + aIndentInc);
			List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
		}

		if (aMode == JsonTextMode.Indent)
			aSB.AppendLine().Append(' ', aIndent);
		aSB.Append(']');
	}
}
// End of JSONArray

public class JsonObject : JsonNode
{
	private bool _inline;
	public Dictionary<string, JsonNode> Dict { get; } = [];

	public override bool Inline
	{
		get => _inline;
		set => _inline = value;
	}

	public override JsonNodeType Tag => JsonNodeType.Object;
	public override bool IsObject => true;


	public override JsonNode this[string aKey]
	{
		get => Dict.TryGetValue(aKey, out var value) ? value : new JsonLazyCreator(this, aKey);
		set
		{
			if (value == null)
				value = JsonNull.CreateOrGet();
			Dict[aKey] = value;
		}
	}

	public override JsonNode this[int aIndex]
	{
		get
		{
			if (aIndex < 0 || aIndex >= Dict.Count)
				return null;
			return Dict.ElementAt(aIndex).Value;
		}
		set
		{
			if (value == null)
				value = JsonNull.CreateOrGet();
			if (aIndex < 0 || aIndex >= Dict.Count)
				return;
			var key = Dict.ElementAt(aIndex).Key;
			Dict[key] = value;
		}
	}

	public override int Count => Dict.Count;

	public override IEnumerable<JsonNode> Children
	{
		get { return Dict.Select(n => n.Value); }
	}

	public override Enumerator GetEnumerator()
	{
		return new Enumerator(Dict.GetEnumerator());
	}

	public override void Add(string aKey, JsonNode aItem)
	{
		if (aItem == null)
			aItem = JsonNull.CreateOrGet();

		if (aKey != null)
			Dict[aKey] = aItem;
		else
			Dict.Add(Guid.NewGuid().ToString(), aItem);
	}

	public override JsonNode Remove(string aKey)
	{
		return !Dict.Remove(aKey, out var tmp) ? null : tmp;
	}

	public override JsonNode Remove(int aIndex)
	{
		if (aIndex < 0 || aIndex >= Dict.Count)
			return null;
		var item = Dict.ElementAt(aIndex);
		Dict.Remove(item.Key);
		return item.Value;
	}

	public override JsonNode Remove(JsonNode aNode)
	{
		try
		{
			var item = Dict.First(k => k.Value == aNode);
			Dict.Remove(item.Key);
			return aNode;
		}
		catch
		{
			return null;
		}
	}

	public override void Clear()
	{
		Dict.Clear();
	}

	public override JsonNode Clone()
	{
		var node = new JsonObject();
		foreach (var n in Dict) node.Add(n.Key, n.Value.Clone());
		return node;
	}

	public override bool HasKey(string aKey)
	{
		return Dict.ContainsKey(aKey);
	}

	public override JsonNode GetValueOrDefault(string aKey, JsonNode aDefault)
	{
		return Dict.GetValueOrDefault(aKey, aDefault);
	}

	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
	{
		aSB.Append('{');
		var first = true;
		if (_inline)
			aMode = JsonTextMode.Compact;
		foreach (var k in Dict)
		{
			if (!first)
				aSB.Append(',');
			first = false;
			if (aMode == JsonTextMode.Indent)
				aSB.AppendLine();
			if (aMode == JsonTextMode.Indent)
				aSB.Append(' ', aIndent + aIndentInc);
			aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
			if (aMode == JsonTextMode.Compact)
				aSB.Append(':');
			else
				aSB.Append(": ");
			k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
		}

		if (aMode == JsonTextMode.Indent)
			aSB.AppendLine().Append(' ', aIndent);
		aSB.Append('}');
	}
}
// End of JSONObject

public class JsonString(string aData) : JsonNode
{
	private string _mData = aData;

	public override JsonNodeType Tag => JsonNodeType.String;
	public override bool IsString => true;


	public override string Value
	{
		get => _mData;
		set => _mData = value;
	}

	public override Enumerator GetEnumerator()
	{
		return new Enumerator();
	}

	public override JsonNode Clone()
	{
		return new JsonString(_mData);
	}

	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
	{
		aSB.Append('\"').Append(Escape(_mData)).Append('\"');
	}

	public override bool Equals(object obj)
	{
		if (base.Equals(obj))
			return true;
		if (obj is string s)
			return _mData == s;
		var s2 = obj as JsonString;
		if (s2 != null)
			return _mData == s2._mData;
		return false;
	}

	public override int GetHashCode()
	{
		return _mData.GetHashCode();
	}

	public override void Clear()
	{
		_mData = "";
	}
}
// End of JSONString

public class JsonNumber : JsonNode
{
	private double _mData;

	public JsonNumber(double aData)
	{
		_mData = aData;
	}

	public JsonNumber(string aData)
	{
		Value = aData;
	}

	public override JsonNodeType Tag => JsonNodeType.Number;
	public override bool IsNumber => true;

	public override string Value
	{
		get => _mData.ToString(CultureInfo.InvariantCulture);
		set
		{
			if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
				_mData = v;
		}
	}

	public override double AsDouble
	{
		get => _mData;
		set => _mData = value;
	}

	public override long AsLong
	{
		get => (long)_mData;
		set => _mData = value;
	}

	public override ulong AsULong
	{
		get => (ulong)_mData;
		set => _mData = value;
	}

	public override Enumerator GetEnumerator()
	{
		return new Enumerator();
	}

	public override JsonNode Clone()
	{
		return new JsonNumber(_mData);
	}

	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
	{
		aSB.Append(Value.ToString(CultureInfo.InvariantCulture));
	}

	private static bool IsNumeric(object value)
	{
		return value is int or uint or float or double or decimal or long or ulong or short or ushort or sbyte or byte;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
			return false;
		if (base.Equals(obj))
			return true;
		var s2 = obj as JsonNumber;
		if (s2 != null)
			return _mData == s2._mData;
		if (IsNumeric(obj))
			return Convert.ToDouble(obj) == _mData;
		return false;
	}

	public override int GetHashCode()
	{
		return _mData.GetHashCode();
	}

	public override void Clear()
	{
		_mData = 0;
	}
}
// End of JSONNumber

public class JsonBool : JsonNode
{
	private bool _mData;

	public JsonBool(bool aData)
	{
		_mData = aData;
	}

	public JsonBool(string aData)
	{
		Value = aData;
	}

	public override JsonNodeType Tag => JsonNodeType.Boolean;
	public override bool IsBoolean => true;

	public override string Value
	{
		get => _mData.ToString();
		set
		{
			if (bool.TryParse(value, out var v))
				_mData = v;
		}
	}

	public override bool AsBool
	{
		get => _mData;
		set => _mData = value;
	}

	public override Enumerator GetEnumerator()
	{
		return new Enumerator();
	}

	public override JsonNode Clone()
	{
		return new JsonBool(_mData);
	}

	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
	{
		aSB.Append(_mData ? "true" : "false");
	}

	public override bool Equals(object obj)
	{
		if (obj is bool boolean)
			return _mData == boolean;
		return false;
	}

	public override int GetHashCode()
	{
		return _mData.GetHashCode();
	}

	public override void Clear()
	{
		_mData = false;
	}
}
// End of JSONBool

public class JsonNull : JsonNode
{
	private static readonly JsonNull MStaticInstance = new();
	public static bool ReuseSameInstance = true;

	private JsonNull()
	{
	}

	public override JsonNodeType Tag => JsonNodeType.NullValue;
	public override bool IsNull => true;

	public override string Value
	{
		get => "null";
		set { }
	}

	public override bool AsBool
	{
		get => false;
		set { }
	}

	public static JsonNull CreateOrGet()
	{
		if (ReuseSameInstance)
			return MStaticInstance;
		return new JsonNull();
	}

	public override Enumerator GetEnumerator()
	{
		return new Enumerator();
	}

	public override JsonNode Clone()
	{
		return CreateOrGet();
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(this, obj))
			return true;
		return obj is JsonNull;
	}

	public override int GetHashCode()
	{
		return 0;
	}

	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
	{
		aSB.Append("null");
	}
}
// End of JSONNull

internal class JsonLazyCreator : JsonNode
{
	private readonly string _mKey;
	private JsonNode _mNode;

	public JsonLazyCreator(JsonNode aNode, string aKey = null)
	{
		_mNode = aNode;
		_mKey = aKey;
	}

	public override JsonNodeType Tag => JsonNodeType.None;

	public override JsonNode this[int aIndex]
	{
		get => new JsonLazyCreator(this);
		set => Set(new JsonArray()).Add(value);
	}

	public override JsonNode this[string aKey]
	{
		get => new JsonLazyCreator(this, aKey);
		set => Set(new JsonObject()).Add(aKey, value);
	}

	public override int AsInt
	{
		get
		{
			Set(new JsonNumber(0));
			return 0;
		}
		set => Set(new JsonNumber(value));
	}

	public override float AsFloat
	{
		get
		{
			Set(new JsonNumber(0.0f));
			return 0.0f;
		}
		set => Set(new JsonNumber(value));
	}

	public override double AsDouble
	{
		get
		{
			Set(new JsonNumber(0.0));
			return 0.0;
		}
		set => Set(new JsonNumber(value));
	}

	public override long AsLong
	{
		get
		{
			if (LongAsString)
				Set(new JsonString("0"));
			else
				Set(new JsonNumber(0.0));
			return 0L;
		}
		set
		{
			if (LongAsString)
				Set(new JsonString(value.ToString(CultureInfo.InvariantCulture)));
			else
				Set(new JsonNumber(value));
		}
	}

	public override ulong AsULong
	{
		get
		{
			if (LongAsString)
				Set(new JsonString("0"));
			else
				Set(new JsonNumber(0.0));
			return 0L;
		}
		set
		{
			if (LongAsString)
				Set(new JsonString(value.ToString(CultureInfo.InvariantCulture)));
			else
				Set(new JsonNumber(value));
		}
	}

	public override bool AsBool
	{
		get
		{
			Set(new JsonBool(false));
			return false;
		}
		set => Set(new JsonBool(value));
	}

	public override JsonArray AsArray => Set(new JsonArray());

	public override JsonObject AsObject => Set(new JsonObject());

	public override Enumerator GetEnumerator()
	{
		return new Enumerator();
	}

	private T Set<T>(T aVal) where T : JsonNode
	{
		if (_mKey == null)
			_mNode.Add(aVal);
		else
			_mNode.Add(_mKey, aVal);
		_mNode = null; // Be GC friendly.
		return aVal;
	}

	public override void Add(JsonNode aItem)
	{
		Set(new JsonArray()).Add(aItem);
	}

	public override void Add(string aKey, JsonNode aItem)
	{
		Set(new JsonObject()).Add(aKey, aItem);
	}

	public static bool operator ==(JsonLazyCreator a, object b)
	{
		if (b == null)
			return true;
		return ReferenceEquals(a, b);
	}

	public static bool operator !=(JsonLazyCreator a, object b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
			return true;
		return ReferenceEquals(this, obj);
	}

	public override int GetHashCode()
	{
		return 0;
	}

	internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JsonTextMode aMode)
	{
		aSB.Append("null");
	}
}
// End of JSONLazyCreator

public static class Json
{
	public static JsonNode Parse(string aJSON)
	{
		return JsonNode.Parse(aJSON);
	}
}