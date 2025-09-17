using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

public class Packet
{
    public PROTOCOL type;

    public ushort body_size;

    public byte[] body = new byte[1024];

    public Packet(PROTOCOL pro, byte[] data = null)
    {
        this.type = pro;
        this.body_size = (ushort)(data != null ? data.Length : 0);
        if (data != null)
            Array.Copy(data, body, this.body_size);
    }

    public Packet(PROTOCOL pro, int param)
    {
        byte[] data = BitConverter.GetBytes(param);

        this.type = pro;
        this.body_size = (ushort)(data == null ? data.Length : 0);
        if (data != null)
            Array.Copy(data, body, this.body_size);
    }
    public Packet(PROTOCOL pro, byte state)
    {
        this.type = pro;
        this.body[0] = state;
    }

    // 파라미터에 직렬화 된 패킷 데이터 복사
    public void Serialize(byte[] dst)
    {
        if (dst.Length < 4 + body_size) throw new ArgumentOutOfRangeException(nameof(dst));

        // 엔디안 정책 결정: little (호스트 바이트 순서)
        byte[] pro = BitConverter.GetBytes((ushort)type);
        byte[] size = BitConverter.GetBytes(body_size);

        dst[0] = pro[0];
        dst[1] = pro[1];
        dst[2] = size[0];
        dst[3] = size[1];

        Array.Copy(this.body, 0, dst, 4, body_size);
    }
}