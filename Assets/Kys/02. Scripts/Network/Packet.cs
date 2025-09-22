using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

public class Packet
{
    public PROTOCOL type;

    public ushort body_size;

    public byte[] body = new byte[1024];

    /// <summary> 데이터를 보내야하는 패킷 생성자 </summary>
    public Packet(PROTOCOL pro)
    {
        this.type = pro;
    }

    /// <summary> 바이트 배열 더하기 </summary>
    public void Write(byte[] data)
    {
        if (data != null)
            Array.Copy(data, 0, body, body_size, data.Length);
        this.body_size += (ushort)data.Length;
    }

    public void Write(byte data)
    {
        this.body[body_size] = data;
        this.body_size++;
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