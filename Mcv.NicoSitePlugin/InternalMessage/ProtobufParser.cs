using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Mcv.NicoSitePlugin.InternalMessage;
public record class Field(ulong No, IValueType Value);
public interface IValueType { }
public class Varint : IValueType
{
    public ulong Value { get; set; }
    public Varint(ulong value)
    {
        Value = value;
    }
    public byte[] ToBytes()
    {
        var list = new List<byte>();
        while (Value > 0)
        {
            var b = (byte)(Value & 0x7f);
            Value >>= 7;
            if (Value > 0)
            {
                b |= 0x80;
            }
            list.Add(b);
        }
        return list.ToArray();
    }
    public static Varint FromBytes(byte[] data)
    {
        var varint = ProtobufParser.ReadAsVarint(data);
        return new Varint(varint.Value);
    }
}
public record class Fixed32(uint Value) : IValueType;
public record class Fixed64(ulong Value) : IValueType;
public class LengthDelimited : IValueType
{
    public byte[] Bytes { get; }
    public LengthDelimited(byte[] bytes)
    {
        Bytes = bytes;

    }
    public byte[] ToBytes()
    {
        var len = new Varint((ulong)Bytes.Length).ToBytes();
        return len.Concat(Bytes).ToArray();
    }
    public static LengthDelimited FromString(string s)
    {
        return new LengthDelimited(Encoding.UTF8.GetBytes(s));
    }
}
public static class ProtobufParser
{
    public static List<Field> Parse(byte[] data)
    {
        var fields = new List<Field>();
        int pos = 0;
        while (pos < data.Length)
        {
            var (header, headerLen) = ReadAsVarint(data[pos..]);
            pos += headerLen;
            var fieldNo = header >> 3;
            var type = header & 0x07;
            //Debug.WriteLine($"no={fieldNo}, type={type}");
            switch (type)
            {
                case 0:
                    {
                        var (val, len) = ReadAsVarint(data[pos..]);
                        pos += len;
                        fields.Add(new Field(fieldNo, new Varint(val)));
                    }
                    break;
                case 1:
                    {
                        var fixed32 = BitConverter.ToUInt32(data[pos..(pos + 4)]);
                        pos += 4;
                        fields.Add(new Field(fieldNo, new Fixed32(fixed32)));
                    }
                    break;
                case 2:
                    {
                        var (length, len) = ReadAsVarint(data[pos..]);
                        pos += len;
                        var bytes = data[pos..(pos + (int)length)];
                        pos += (int)length;
                        fields.Add(new Field(fieldNo, new LengthDelimited(bytes.ToArray())));
                    }
                    break;
                case 5:
                    {
                        var fixed64 = BitConverter.ToUInt64(data[pos..(pos + 8)]);
                        pos += 8;
                        fields.Add(new Field(fieldNo, new Fixed64(fixed64)));
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid type");
            }
        }
        return fields;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static (ulong Value, int Length) ReadAsVarint(byte[] data)
    {
        ulong result = 0;
        int shift = 0;
        int len = 0;
        foreach (var b in data)
        {
            len++;
            result |= (ulong)(b & 0x7f) << shift;
            if ((b & 0x80) == 0)
            {
                return (result, len);
            }
            shift += 7;
        }
        throw new InvalidOperationException("Invalid varint");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fieldNo"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static byte[] CreateHeader(ulong fieldNo, uint type)
    {
        var n = fieldNo << 3;
        return new Varint(n | type).ToBytes();
    }
}
class ChunkedEntry
{
    public MessageSegment? Segment { get; }
    public BackwardSegment? Backward { get; }
    public MessageSegment? Previous { get; }
    public ReadyForNext? Next { get; }
    public ChunkedEntry(MessageSegment? segment, BackwardSegment? backward, MessageSegment? previous, ReadyForNext? next)
    {
        Segment = segment;
        Backward = backward;
        Previous = previous;
        Next = next;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<byte[]> SplitData(byte[] data)
    {
        var list = new List<byte[]>();
        int pos = 0;
        while (pos < data.Length)
        {
            var (length, len) = ProtobufParser.ReadAsVarint(data[pos..]);
            pos += len;
            list.Add(data[pos..(pos + (int)length)]);
            pos += (int)length;
        }
        return list;
    }
    public static List<ChunkedEntry> Create(byte[] data)
    {
        var list = new List<ChunkedEntry>();

        var subs = SplitData(data);
        foreach (var sub in subs)
        {
            var fields = ProtobufParser.Parse(sub);
            MessageSegment? segment = null;
            BackwardSegment? backward = null;
            MessageSegment? previous = null;
            ReadyForNext? next = null;
            foreach (var field in fields)
            {
                switch (field.No)
                {
                    case 1:
                        segment = MessageSegment.Create(((LengthDelimited)field.Value).Bytes);
                        break;
                    case 2:
                        backward = BackwardSegment.Create(((LengthDelimited)field.Value).Bytes);
                        break;
                    case 3:
                        previous = MessageSegment.Create(((LengthDelimited)field.Value).Bytes);
                        break;
                    case 4:
                        next = ReadyForNext.Create(((LengthDelimited)field.Value).Bytes);
                        break;
                }
            }
            list.Add(new ChunkedEntry(segment, backward, previous, next));
        }
        return list;
    }
}
class BackwardSegment
{
    public string Segment { get; }
    public ulong Until { get; }
    public string Snapshot { get; }
    public BackwardSegment(ulong until, string segment, string snapshot)
    {
        Until = until;
        Segment = segment;
        Snapshot = snapshot;
    }
    public static BackwardSegment Create(byte[] bytes)
    {
        var b = ProtobufParser.Parse(bytes);
        var b0 = ((LengthDelimited)b[0].Value).Bytes;
        var b0_fields = ProtobufParser.Parse(b0);
        var b00 = ((Varint)b0_fields[0].Value).Value;

        var b1 = ((LengthDelimited)b[1].Value).Bytes;
        var b1_fields = ProtobufParser.Parse(b1);
        var b1_s = Encoding.UTF8.GetString(((LengthDelimited)b1_fields[0].Value).Bytes);

        var b2 = ((LengthDelimited)b[2].Value).Bytes;
        var b2_fields = ProtobufParser.Parse(b2);
        var b2_s = Encoding.UTF8.GetString(((LengthDelimited)b2_fields[0].Value).Bytes);
        return new BackwardSegment(b00, b1_s, b2_s);
    }
}
class MessageSegment
{
    public ulong From { get; }
    public ulong Until { get; }
    public string Uri { get; }
    private MessageSegment(ulong from, ulong until, string uri)
    {
        From = from;
        Until = until;
        Uri = uri;
    }
    public static MessageSegment Create(byte[] bytes)
    {
        var b = ProtobufParser.Parse(bytes);

        var c0 = ((LengthDelimited)b[0].Value).Bytes;
        var c0a = ProtobufParser.Parse(c0);
        var b0_val = ((Varint)c0a[0].Value).Value;

        var c1 = ((LengthDelimited)b[1].Value).Bytes;
        var c1a = ProtobufParser.Parse(c1);
        var b1_val = ((Varint)c1a[0].Value).Value;

        var c2 = ((LengthDelimited)b[2].Value).Bytes;
        var c2a = Encoding.UTF8.GetString(c2);
        return new MessageSegment(b0_val, b1_val, c2a);
    }
}
class ReadyForNext
{
    public long At { get; }
    private ReadyForNext(long at)
    {
        At = at;
    }
    public static ReadyForNext Create(byte[] bytes)
    {
        var kkk = ProtobufParser.Parse(bytes);
        var at = (long)((Varint)kkk[0].Value).Value;
        return new ReadyForNext(at);
    }
}
class ChunkedMessage
{
    public Meta Meta { get; }
    public NicoliveMessage? Message { get; }
    public NicoliveState? State { get; }
    public Signal? Signal { get; }
    private ChunkedMessage(Meta meta, NicoliveMessage? message, NicoliveState? state, Signal? signal)
    {
        Meta = meta;
        Message = message;
        State = state;
        Signal = signal;
    }
    public static List<ChunkedMessage> Create2(byte[] bytes)
    {
        var list = ChunkedEntry.SplitData(bytes);
        return list.Select(a => Create(a)).ToList();
    }
    public static ChunkedMessage Create(byte[] bytes)
    {
        Meta? meta = null;
        NicoliveMessage? message = null;
        NicoliveState? state = null;
        Signal? signal = null;
        var c = ProtobufParser.Parse(bytes);
        foreach (var ca in c)
        {
            switch (ca.No)
            {
                case 1:
                    meta = Meta.Create(((LengthDelimited)ca.Value).Bytes);
                    break;
                case 2:
                    message = NicoliveMessage.Create(((LengthDelimited)ca.Value).Bytes);
                    break;
                case 4:
                    state = NicoliveState.Create(((LengthDelimited)ca.Value).Bytes);
                    break;
                case 5:
                    signal = (Signal)((Varint)ca.Value).Value;
                    break;
                default:
                    break;
            }
        }


        //var c0 = ProtobufParser.Parse(((LengthDelimited)c[0].Value).Bytes);
        //var s = Encoding.UTF8.GetString(((LengthDelimited)c0[0].Value).Bytes);
        //Debug.WriteLine(s);
        //var c01t = Timestamp.Create(((LengthDelimited)c0[1].Value).Bytes);

        //var c02 = ProtobufParser.Parse(((LengthDelimited)c0[2].Value).Bytes);


        return new ChunkedMessage(meta!, message, state, signal);
    }
    //public static ChunkedMessage Create(byte[] bytes)
    //{
    //    Meta? meta = null;
    //    NicoliveMessage? message = null;
    //    NicoliveState? state = null;
    //    var a0 = ProtobufParser.Parse(bytes);
    //    foreach (var entry in a0)
    //    {
    //        switch (entry.No)
    //        {
    //            case 1:
    //                meta = Meta.Create(((LengthDelimited)entry.Value).Bytes);
    //                break;
    //            case 2:
    //                {
    //                    var a = ProtobufParser.Parse(((LengthDelimited)entry.Value).Bytes);
    //                    var b = ProtobufParser.Parse(((LengthDelimited)a[0].Value).Bytes);
    //                    foreach (var part in b)
    //                    {
    //                        switch (part.No)
    //                        {
    //                            case 1:
    //                                {
    //                                    var s = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
    //                                }
    //                                break;
    //                            case 7:
    //                                {
    //                                    if (((LengthDelimited)part.Value).Bytes.Length > 0)
    //                                    {

    //                                    }
    //                                }
    //                                break;
    //                            case 8:
    //                                {
    //                                    var n = ((Varint)part.Value).Value;
    //                                }
    //                                break;
    //                            case 9:
    //                                {

    //                                }
    //                                break;
    //                            case 13:
    //                                {

    //                                }
    //                                break;
    //                            case 17:
    //                                {

    //                                }
    //                                break;
    //                            case 18:
    //                                {

    //                                }
    //                                break;
    //                            case 19:
    //                                {

    //                                }
    //                                break;
    //                            case 20:
    //                                {

    //                                }
    //                                break;
    //                            default:
    //                                break;
    //                        }
    //                    }
    //                }
    //                break;
    //            case 4:
    //                break;
    //            case 5:
    //                //signal
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //    return new ChunkedMessage(meta, message, state);
    //}
}
enum Signal
{
    Flushed,
}
class Meta
{
    public string Id { get; }
    public Timestamp At { get; }
    public long liveId { get; }
    private Meta(string id, Timestamp at, long liveId)
    {
        Id = id;
        At = at;
        this.liveId = liveId;
    }
    public static Meta Create(byte[] bytes)
    {
        var a00 = ProtobufParser.Parse(bytes);
        var id = Encoding.UTF8.GetString(((LengthDelimited)a00[0].Value).Bytes);
        var at = Timestamp.Create(((LengthDelimited)a00[1].Value).Bytes);
        //var a001 = ProtobufParser.Parse(();
        //var a0010_val = ((Varint)a001[0].Value).Value;
        //var a0011_val = ((Varint)a001[1].Value).Value;
        var a002 = ProtobufParser.Parse(((LengthDelimited)a00[2].Value).Bytes);
        var a0020 = ProtobufParser.Parse(((LengthDelimited)a002[0].Value).Bytes);
        var liveId = (long)((Varint)a0020[0].Value).Value;
        return new Meta(id, at, liveId);
    }
}
enum AccountStatus
{
    Standard = 0,
    Premium = 1,
}
enum Pos
{
    Naka = 0,
    Shita = 1,
    Ue = 2,
}
enum Size
{
    Medium = 0,
    Small = 1,
    Big = 2,
}
enum ColorName
{
    White = 0,
    Red = 1,
    Pink = 2,
    Orange = 3,
    Yellow = 4,
    Green = 5,
    Cyan = 6,
    Blue = 7,
    Purple = 8,
    Black = 9,
    White2 = 10,
    Red2 = 11,
    Pink2 = 12,
    Orange2 = 13,
    Yellow2 = 14,
    Green2 = 15,
    Cyan2 = 16,
    Blue2 = 17,
    Purple2 = 18,
    Black2 = 19,
}
enum Font
{
    Defont = 0,
    Mincho = 1,
    Gothic = 2,
}
enum Opacity
{
    Normal = 0,
    Translucent = 1,
}
class Chat
{
    public string Content { get; }
    public string Name { get; }
    public int Vpos { get; }
    public AccountStatus AccountStatus { get; }
    public long? RawUserId { get; }
    public string? HashedUserId { get; }
    public object Modifier { get; }
    public int No { get; }
    public Chat(string content, string name, int vpos, AccountStatus accountStatus, long? rawUserId, string? hashedUserId, object modifier, int no)
    {
        Content = content;
        Name = name;
        Vpos = vpos;
        AccountStatus = accountStatus;
        RawUserId = rawUserId;
        HashedUserId = hashedUserId;
        Modifier = modifier;
        No = no;
    }
    public static Chat Create(byte[] bytes)
    {
        var c10 = ProtobufParser.Parse(bytes);
        string? content = null;
        string? name = null;
        int? vpos = null;
        AccountStatus? ac = null;
        long? raw_user_id = null;
        string? hashed_user_id = null;
        object? modifier = null;
        int? no = null;
        foreach (var p in c10)
        {
            switch (p.No)
            {
                case 1:
                    content = Encoding.UTF8.GetString(((LengthDelimited)p.Value).Bytes);
                    break;
                case 2:
                    name = Encoding.UTF8.GetString(((LengthDelimited)p.Value).Bytes);
                    break;
                case 3:
                    vpos = (int)((Varint)p.Value).Value;
                    break;
                case 4:
                    var acc = (int)((Varint)p.Value).Value;
                    ac = (AccountStatus)acc;
                    break;
                case 5:
                    raw_user_id = (int)((Varint)p.Value).Value;
                    break;
                case 6:
                    hashed_user_id = Encoding.UTF8.GetString(((LengthDelimited)p.Value).Bytes);
                    break;
                case 7://modifier
                    var n = ((LengthDelimited)p.Value).Bytes;
                    if (n.Length > 0)
                    {
                        var ttt = ProtobufParser.Parse(n);
                        switch (ttt[0].No)
                        {
                            case 1://
                                var position = (Pos)((Varint)ttt[0].Value).Value;
                                break;
                            case 2:
                                var size = (Size)((Varint)ttt[0].Value).Value;
                                break;
                            case 3:
                                var namedColor = (ColorName)((Varint)ttt[0].Value).Value;
                                break;
                            case 4://full_color
                                var full_color = ProtobufParser.Parse(((LengthDelimited)ttt[0].Value).Bytes);
                                foreach (var full_color_part in full_color)
                                {
                                    switch (full_color_part.No)
                                    {
                                        case 1:
                                            var r = ((Varint)full_color_part.Value).Value;
                                            break;
                                        case 2:
                                            var g = ((Varint)full_color_part.Value).Value;
                                            break;
                                        case 3:
                                            var b = ((Varint)full_color_part.Value).Value;
                                            break;
                                    }
                                }
                                break;
                            case 5://font
                                break;
                            case 6://opacity
                                {
                                    var b = ((Varint)ttt[0].Value).Value;

                                }
                                break;
                            default:
                                break;
                        }

                    }
                    modifier = new object();
                    break;
                case 8:
                    no = (int)((Varint)p.Value).Value;
                    break;
                default:
                    break;

            }
        }
        if (name is null)
        {
            name = "";
        }
        if (ac is null)
        {
            ac = AccountStatus.Standard;
        }
        return new Chat(content!, name!, vpos!.Value, ac!.Value, raw_user_id, hashed_user_id, modifier!, no!.Value);
    }
}
class SimpleNotification
{
    public string? Ichiba { get; private set; }
    public string? Quote { get; private set; }
    public string? Emotion { get; private set; }
    public string? Cruise { get; private set; }
    public string? ProgramExtended { get; private set; }
    public string? RankingIn { get; private set; }
    public string? RankingUpdated { get; private set; }
    public string? Visited { get; private set; }
    public static SimpleNotification Create(byte[] bytes)
    {
        var z = ProtobufParser.Parse(bytes);
        foreach (var part in z)
        {
            switch (part.No)
            {
                case 1:
                    //ichiba
                    var a1 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { Ichiba = a1 };
                case 2:
                    //quote
                    var a2 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { Quote = a2 };
                case 3:
                    //emotion
                    var a3 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { Emotion = a3 };
                case 4:
                    //cruise
                    var a4 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { Cruise = a4 };
                case 5:
                    //program_extended
                    var a5 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { ProgramExtended = a5 };
                case 6:
                    //ranking_in
                    var a6 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { RankingIn = a6 };
                case 7:
                    //visited
                    var a7 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { Visited = a7 };
                case 8:
                    //ranking_updated
                    var a8 = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    return new SimpleNotification { RankingUpdated = a8 };
                default:
                    return new SimpleNotification();
            }
        }
        return new SimpleNotification();
    }
}
class Gift
{
    public string ItemId { get; }
    public long? AdvertiserUserId { get; }
    public string AdvertiserName { get; }
    public long Point { get; }
    public string? Message { get; }//nullの場合があった
    public string ItemName { get; }
    public long? ContributionRank { get; }
    public Gift(string itemId, long? advertiserUserId, string advertiserName, long point, string? message, string itemName, long? contributionRank)
    {
        ItemId = itemId;
        AdvertiserUserId = advertiserUserId;
        AdvertiserName = advertiserName;
        Point = point;
        Message = message;
        ItemName = itemName;
        ContributionRank = contributionRank;
    }
    public static Gift Create(byte[] bytes)
    {
        string? itemId = null;
        long? advertiserUserId = null;
        string? advertiserName = null;
        long? point = null;
        string? message = null;
        string? itemName = null;
        long? contributionRank = null;
        var parts = ProtobufParser.Parse(bytes);
        foreach (var part in parts)
        {
            switch (part.No)
            {
                case 1://item_id
                    itemId = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    break;
                case 2:
                    advertiserUserId = (long)((Varint)part.Value).Value;
                    break;
                case 3:
                    advertiserName = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    break;
                case 4:
                    point = (long)((Varint)part.Value).Value;
                    break;
                case 5://message
                    message = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    break;
                case 6:
                    itemName = Encoding.UTF8.GetString(((LengthDelimited)part.Value).Bytes);
                    break;
                case 7:
                    contributionRank = (long)((Varint)part.Value).Value;
                    break;
            }
        }

        return new Gift(itemId!, advertiserUserId, advertiserName!, point!.Value, message, itemName!, contributionRank);
    }
}
class Tag
{
    public string Text { get; private set; }
    public bool Locked { get; private set; }
    public bool Reserved { get; private set; }
    public string? NicopediaUri { get; private set; }
    private Tag(string text, bool locked, bool reserved, string? nicopediaUri)
    {
        Text = text;
        Locked = locked;
        Reserved = reserved;
        NicopediaUri = nicopediaUri;
    }
    public static Tag Create(byte[] bytes)
    {
        string? text = null;
        bool? locked = null;
        bool? reserved = null;
        string? nicopediaUri = null;
        var ms = ProtobufParser.Parse(bytes);
        foreach (var m in ms)
        {
            switch (m.No)
            {
                case 1:
                    text = Encoding.UTF8.GetString(((LengthDelimited)m.Value).Bytes);
                    break;
                case 2:
                    locked = ((Varint)m.Value).Value == 1;
                    break;
                case 3:
                    reserved = ((Varint)m.Value).Value == 1;
                    break;
                case 4:
                    nicopediaUri = Encoding.UTF8.GetString(((LengthDelimited)m.Value).Bytes);
                    break;
                default:
                    break;
            }
        }
        return new Tag(text!, locked ?? false, reserved ?? false, nicopediaUri);
    }
}
class TagUpdated
{
    public static TagUpdated Create(byte[] bytes)
    {
        //0x0A, 0x53, 0x0A, 0x0C, 0xE4, 0xB8, 0x80, 0xE8, 0x88, 0xAC, 0xE4, 0xBC, 0x9A, 0xE5, 0x93, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x3F, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x34, 0x25, 0x42, 0x38, 0x25, 0x38, 0x30, 0x25, 0x45, 0x38, 0x25, 0x38, 0x38, 0x25, 0x41, 0x43, 0x25, 0x45, 0x34, 0x25, 0x42, 0x43, 0x25, 0x39, 0x41, 0x25, 0x45, 0x35, 0x25, 0x39, 0x33, 0x25, 0x41, 0x31, 0x0A, 0x3F, 0x0A, 0x08, 0x48, 0x44, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x2F, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x48, 0x44, 0x25, 0x45, 0x39, 0x25, 0x38, 0x35, 0x25, 0x38, 0x44, 0x25, 0x45, 0x34, 0x25, 0x42, 0x46, 0x25, 0x41, 0x31, 0x0A, 0x8F, 0x01, 0x0A, 0x1B, 0xE3, 0x82, 0xB9, 0xE3, 0x83, 0x9E, 0xE3, 0x83, 0xBC, 0xE3, 0x83, 0x88, 0xE3, 0x83, 0x95, 0xE3, 0x82, 0xA9, 0xE3, 0x83, 0xB3, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x6C, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x42, 0x39, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x45, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x38, 0x38, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x35, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x39, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x33, 0x25, 0x45, 0x39, 0x25, 0x38, 0x35, 0x25, 0x38, 0x44, 0x25, 0x45, 0x34, 0x25, 0x42, 0x46, 0x25, 0x41, 0x31, 0x0A, 0x10, 0x0A, 0x0C, 0xE9, 0x9B, 0x91, 0xE8, 0xAB, 0x87, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x0A, 0x13, 0x0A, 0x0F, 0xE5, 0xA5, 0xB3, 0xE6, 0x80, 0xA7, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0xE8, 0x80, 0x85, 0x10, 0x01, 0x0A, 0x81, 0x01, 0x0A, 0x18, 0xE3, 0x82, 0xB7, 0xE3, 0x83, 0xAB, 0xE3, 0x83, 0x90, 0xE3, 0x83, 0xBC, 0xE3, 0x82, 0xA6, 0xE3, 0x82, 0xA4, 0xE3, 0x83, 0xBC, 0xE3, 0x82, 0xAF, 0x10, 0x01, 0x22, 0x63, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x42, 0x37, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x41, 0x42, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x30, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x36, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x34, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x46, 0x0A, 0x29, 0x0A, 0x27, 0xE7, 0xA7, 0x8B, 0xE5, 0x88, 0x86, 0xE3, 0x81, 0xAE, 0xE6, 0x97, 0xA5, 0xEF, 0xBC, 0x88, 0xE3, 0x81, 0x97, 0xE3, 0x82, 0x85, 0xE3, 0x82, 0x93, 0xE3, 0x81, 0xB6, 0xE3, 0x82, 0x93, 0xE3, 0x81, 0xAE, 0xE3, 0x81, 0xB2, 0xEF, 0xBC, 0x89, 0x0A, 0x43, 0x0A, 0x09, 0xE3, 0x81, 0xBE, 0xE3, 0x81, 0x81, 0xE3, 0x82, 0x8B, 0x22, 0x36, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x6C, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x31, 0x25, 0x42, 0x45, 0x25, 0x45, 0x33, 0x25, 0x38, 0x31, 0x25, 0x38, 0x31, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x38, 0x42, 0x0A, 0x4F, 0x0A, 0x0C, 0xE3, 0x81, 0x8B, 0xE3, 0x82, 0x8F, 0xE3, 0x81, 0x84, 0xE3, 0x81, 0x84, 0x22, 0x3F, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x31, 0x25, 0x38, 0x42, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x38, 0x46, 0x25, 0x45, 0x33, 0x25, 0x38, 0x31, 0x25, 0x38, 0x34, 0x25, 0x45, 0x33, 0x25, 0x38, 0x31, 0x25, 0x38, 0x34
        //0x0A, 0x53, 0x0A, 0x0C, 0xE4, 0xB8, 0x80, 0xE8, 0x88, 0xAC, 0xE4, 0xBC, 0x9A, 0xE5, 0x93, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x3F, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x34, 0x25, 0x42, 0x38, 0x25, 0x38, 0x30, 0x25, 0x45, 0x38, 0x25, 0x38, 0x38, 0x25, 0x41, 0x43, 0x25, 0x45, 0x34, 0x25, 0x42, 0x43, 0x25, 0x39, 0x41, 0x25, 0x45, 0x35, 0x25, 0x39, 0x33, 0x25, 0x41, 0x31, 0x0A, 0x3F, 0x0A, 0x08, 0x48, 0x44, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x2F, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x48, 0x44, 0x25, 0x45, 0x39, 0x25, 0x38, 0x35, 0x25, 0x38, 0x44, 0x25, 0x45, 0x34, 0x25, 0x42, 0x46, 0x25, 0x41, 0x31, 0x0A, 0x8F, 0x01, 0x0A, 0x1B, 0xE3, 0x82, 0xB9, 0xE3, 0x83, 0x9E, 0xE3, 0x83, 0xBC, 0xE3, 0x83, 0x88, 0xE3, 0x83, 0x95, 0xE3, 0x82, 0xA9, 0xE3, 0x83, 0xB3, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x6C, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x42, 0x39, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x45, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x38, 0x38, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x35, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x39, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x33, 0x25, 0x45, 0x39, 0x25, 0x38, 0x35, 0x25, 0x38, 0x44, 0x25, 0x45, 0x34, 0x25, 0x42, 0x46, 0x25, 0x41, 0x31, 0x0A, 0x10, 0x0A, 0x0C, 0xE9, 0x9B, 0x91, 0xE8, 0xAB, 0x87, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x0A, 0x13, 0x0A, 0x0F, 0xE5, 0xA5, 0xB3, 0xE6, 0x80, 0xA7, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0xE8, 0x80, 0x85, 0x10, 0x01, 0x0A, 0x81, 0x01, 0x0A, 0x18, 0xE3, 0x82, 0xB7, 0xE3, 0x83, 0xAB, 0xE3, 0x83, 0x90, 0xE3, 0x83, 0xBC, 0xE3, 0x82, 0xA6, 0xE3, 0x82, 0xA4, 0xE3, 0x83, 0xBC, 0xE3, 0x82, 0xAF, 0x10, 0x01, 0x22, 0x63, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x42, 0x37, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x41, 0x42, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x30, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x36, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x34, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x46, 0x0A, 0x29, 0x0A, 0x27, 0xE7, 0xA7, 0x8B, 0xE5, 0x88, 0x86, 0xE3, 0x81, 0xAE, 0xE6, 0x97, 0xA5, 0xEF, 0xBC, 0x88, 0xE3, 0x81, 0x97, 0xE3, 0x82, 0x85, 0xE3, 0x82, 0x93, 0xE3, 0x81, 0xB6, 0xE3, 0x82, 0x93, 0xE3, 0x81, 0xAE, 0xE3, 0x81, 0xB2, 0xEF, 0xBC, 0x89, 0x0A, 0x0E, 0x0A, 0x0C, 0xE3, 0x81, 0xBE, 0xE3, 0x83, 0xBC, 0xE3, 0x81, 0xBF, 0xE3, 0x83, 0xBC, 0x0A, 0x1A, 0x0A, 0x18, 0xE3, 0x82, 0xBB, 0xE3, 0x82, 0xAF, 0xE3, 0x82, 0xB7, 0xE3, 0x83, 0xBC, 0xE3, 0x81, 0x8A, 0xE5, 0xA7, 0x89, 0xE3, 0x81, 0x95, 0xE3, 0x82, 0x93
        //0x0A, 0x53, 0x0A, 0x0C, 0xE4, 0xB8, 0x80, 0xE8, 0x88, 0xAC, 0xE4, 0xBC, 0x9A, 0xE5, 0x93, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x3F, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x34, 0x25, 0x42, 0x38, 0x25, 0x38, 0x30, 0x25, 0x45, 0x38, 0x25, 0x38, 0x38, 0x25, 0x41, 0x43, 0x25, 0x45, 0x34, 0x25, 0x42, 0x43, 0x25, 0x39, 0x41, 0x25, 0x45, 0x35, 0x25, 0x39, 0x33, 0x25, 0x41, 0x31, 0x0A, 0x3F, 0x0A, 0x08, 0x48, 0x44, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x2F, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x48, 0x44, 0x25, 0x45, 0x39, 0x25, 0x38, 0x35, 0x25, 0x38, 0x44, 0x25, 0x45, 0x34, 0x25, 0x42, 0x46, 0x25, 0x41, 0x31, 0x0A, 0x8F, 0x01, 0x0A, 0x1B, 0xE3, 0x82, 0xB9, 0xE3, 0x83, 0x9E, 0xE3, 0x83, 0xBC, 0xE3, 0x83, 0x88, 0xE3, 0x83, 0x95, 0xE3, 0x82, 0xA9, 0xE3, 0x83, 0xB3, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x18, 0x01, 0x22, 0x6C, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x42, 0x39, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x45, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x38, 0x38, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x35, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x39, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x33, 0x25, 0x45, 0x39, 0x25, 0x38, 0x35, 0x25, 0x38, 0x44, 0x25, 0x45, 0x34, 0x25, 0x42, 0x46, 0x25, 0x41, 0x31, 0x0A, 0x10, 0x0A, 0x0C, 0xE9, 0x9B, 0x91, 0xE8, 0xAB, 0x87, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0x10, 0x01, 0x0A, 0x13, 0x0A, 0x0F, 0xE5, 0xA5, 0xB3, 0xE6, 0x80, 0xA7, 0xE9, 0x85, 0x8D, 0xE4, 0xBF, 0xA1, 0xE8, 0x80, 0x85, 0x10, 0x01, 0x0A, 0x81, 0x01, 0x0A, 0x18, 0xE3, 0x82, 0xB7, 0xE3, 0x83, 0xAB, 0xE3, 0x83, 0x90, 0xE3, 0x83, 0xBC, 0xE3, 0x82, 0xA6, 0xE3, 0x82, 0xA4, 0xE3, 0x83, 0xBC, 0xE3, 0x82, 0xAF, 0x10, 0x01, 0x22, 0x63, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3A, 0x2F, 0x2F, 0x64, 0x69, 0x63, 0x2E, 0x6E, 0x69, 0x63, 0x6F, 0x76, 0x69, 0x64, 0x65, 0x6F, 0x2E, 0x6A, 0x70, 0x2F, 0x61, 0x2F, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x42, 0x37, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x41, 0x42, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x39, 0x30, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x36, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x34, 0x25, 0x45, 0x33, 0x25, 0x38, 0x33, 0x25, 0x42, 0x43, 0x25, 0x45, 0x33, 0x25, 0x38, 0x32, 0x25, 0x41, 0x46
        var tags = new List<Tag>();
        var ns = ProtobufParser.Parse(bytes);
        foreach (var n in ns)
        {
            switch (n.No)
            {
                case 1://tags
                    tags.Add(Tag.Create(((LengthDelimited)n.Value).Bytes));
                    break;
                case 2://owner_locked
                    break;
                default:
                    break;
            }
        }
        return new TagUpdated();
    }
}
class Nicoad
{
    public static Nicoad Create(byte[] bytes)
    {
        var z = ProtobufParser.Parse(bytes);
        foreach (var za in z)
        {
            switch (za.No)
            {
                case 2:
                    var zav = ProtobufParser.Parse(((LengthDelimited)za.Value).Bytes);
                    int? totalAdPoint = null;
                    string? message = null;
                    foreach (var zavk in zav)
                    {
                        switch (zavk.No)
                        {
                            case 1:
                                totalAdPoint = (int)((Varint)zavk.Value).Value;
                                break;
                            case 2:
                                message = Encoding.UTF8.GetString(((LengthDelimited)zavk.Value).Bytes);
                                //【広告貢献1位】シガラフさんが2000ptニコニ広告しました「ガンプラを愛するすべての人へ　あなたはSEED？それともディスティニー？」
                                //【広告貢献4位】ぐるぐるさんが800ptニコニ広告しました
                                //【広告貢献4位】ぐるぐるさんが800ptニコニ広告しました「もう一回、遊べるドン！！」
                                //758さんが400ptニコニ広告しました
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        return new Nicoad();
    }
}
class NicoliveMessage
{
    public Chat? Chat { get; private set; }
    public SimpleNotification? SimpleNotification { get; private set; }
    public Gift? Gift { get; private set; }
    public Nicoad? Nicoad { get; private set; }
    //GameUpdate
    public TagUpdated? TagUpdated { get; private set; }
    //ModeratorUpdated
    //SSNGUpdated
    //OverflowedChat
    public static NicoliveMessage Create(byte[] bytes)
    {
        var m = new NicoliveMessage();
        var data = ProtobufParser.Parse(bytes);
        foreach (var part in data)
        {
            switch (part.No)
            {
                case 1://chat
                    m.Chat = Chat.Create(((LengthDelimited)part.Value).Bytes);
                    break;
                case 7://simple_notification
                    m.SimpleNotification = SimpleNotification.Create(((LengthDelimited)part.Value).Bytes);
                    break;
                case 8://gift
                    m.Gift = Gift.Create(((LengthDelimited)part.Value).Bytes);
                    break;
                case 9://nicoad
                    m.Nicoad = Nicoad.Create(((LengthDelimited)part.Value).Bytes);
                    break;
                case 13://game_update
                    break;
                case 17://tag_updated
                    m.TagUpdated = TagUpdated.Create(((LengthDelimited)part.Value).Bytes);
                    break;
                case 18://moderator_updated
                    break;
                case 19://ssng_updated
                    break;
                case 20://overflowed_chat
                    break;
                default:
                    break;
            }
        }

        return m;
    }
}
enum ProgramState
{
    Unknown = 0,
    Ended = 1,
}
class Modifier
{
    public static Modifier Create(byte[] bytes)
    {
        return new Modifier();
    }
}
class NicoliveState
{
    public ProgramState? State { get; }
    public NicoliveState(ProgramState? state)
    {
        State = state;
    }

    public static NicoliveState Create(byte[] bytes)
    {
        ProgramState? state = null;
        var a = ProtobufParser.Parse(bytes);
        switch (a[0].No)
        {
            case 4://marquee
                {
                    var b = ProtobufParser.Parse(((LengthDelimited)(a[0].Value)).Bytes);
                    var c = ProtobufParser.Parse(((LengthDelimited)b[0].Value).Bytes);
                    foreach (var part in c)
                    {
                        switch (part.No)
                        {
                            case 1://operator_comment
                                var operatorComment = ProtobufParser.Parse(((LengthDelimited)part.Value).Bytes);
                                foreach (var opPart in operatorComment)
                                {
                                    switch (opPart.No)
                                    {
                                        case 1://content
                                            var content = Encoding.UTF8.GetString(((LengthDelimited)opPart.Value).Bytes);
                                            break;
                                        case 2://name
                                            var name = Encoding.UTF8.GetString(((LengthDelimited)opPart.Value).Bytes);
                                            break;
                                        case 3://modifier
                                            var modifier = Modifier.Create(((LengthDelimited)opPart.Value).Bytes);
                                            break;
                                        case 4://link
                                            var link = Encoding.UTF8.GetString(((LengthDelimited)opPart.Value).Bytes);
                                            break;

                                    }
                                }
                                break;
                            case 3://duration
                                var duration = ProtobufParser.Parse(((LengthDelimited)part.Value).Bytes);
                                var n = (long)((Varint)duration[0].Value).Value;
                                break;
                        }
                    }
                }
                break;
            case 9:
                {
                    var b = ProtobufParser.Parse(((LengthDelimited)(a[0].Value)).Bytes);
                    state = (ProgramState)((Varint)b[0].Value).Value;
                }
                break;
            default:
                break;
        }

        return new NicoliveState(state);
    }
}
class Timestamp
{
    public long Seconds { get; }
    public int Nanos { get; }
    public Timestamp(long seconds, int nanos)
    {
        Seconds = seconds;
        Nanos = nanos;
    }

    public static Timestamp Create(byte[] bytes)
    {
        var a001 = ProtobufParser.Parse(bytes);
        var a0010_val = ((Varint)a001[0].Value).Value;
        var a0011_val = ((Varint)a001[1].Value).Value;
        return new Timestamp((long)a0010_val, (int)a0011_val);
    }
}