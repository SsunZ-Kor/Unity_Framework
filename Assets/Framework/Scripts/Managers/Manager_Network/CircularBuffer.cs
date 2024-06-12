using UnityEngine;
using System.Collections;
using System.Text;
using System;

public class CircularBuffer
{
    public int m_iBufSize;
    public int m_iHeadPos;
    public int m_iTailPos;
    public byte[] m_pBuffer;
    public byte[] m_pTempBuffer;

    public CircularBuffer(int size)
    {
        m_iBufSize = size;
        m_iHeadPos = 0;
        m_iTailPos = 0;
        m_pBuffer = new byte[size];
        m_pTempBuffer = new byte[2];
    }

    public void Reset()
    {
        m_iHeadPos = 0;
        m_iTailPos = 0;
    }

    public void GetData( byte[] pData , int len )
    {
        if (len < m_iBufSize - m_iHeadPos)
        {
            System.Buffer.BlockCopy(m_pBuffer, m_iHeadPos, pData, 0, len);
        }
        else
        {
            int fc, sc;
            fc = m_iBufSize - m_iHeadPos;
            sc = len - fc;
            System.Buffer.BlockCopy(m_pBuffer, m_iHeadPos, pData, 0, fc);
            if ( sc > 0 )
            {
                System.Buffer.BlockCopy(m_pBuffer, 0, pData, fc, sc);
            }
        }
    }

    public int GetValidCount()
    {
        int count = m_iTailPos - m_iHeadPos;
        if (count < 0)
        {
            count = m_iBufSize + count;
        }

        if( count > 0 )
        {
        }
        return count;
    }

    public bool IsIndexOverFlow(int len)
    {
        if (len + m_iTailPos >= m_iBufSize)
            return true;
        else
            return false;
    }

    public void PutData(byte[] pData, int len)
    {
        if (IsIndexOverFlow(len))
        {
            int FirstCopyLen = m_iBufSize - m_iTailPos;
            int SecondCopyLen = len - FirstCopyLen;
            System.Buffer.BlockCopy(pData, 0, m_pBuffer, m_iTailPos, FirstCopyLen);
            if (SecondCopyLen != 0)
            {
                System.Buffer.BlockCopy(pData, FirstCopyLen, m_pBuffer, 0, SecondCopyLen);
                m_iTailPos = SecondCopyLen;
            }
            else
            {
                m_iTailPos = 0;
            }
        }
        else
        {
            System.Buffer.BlockCopy(pData, 0, m_pBuffer, m_iTailPos, len);
            m_iTailPos += len;
        }
    }

    public bool HeadIncrease(int increasement)
    {
        m_iHeadPos += increasement;
        m_iHeadPos %= m_iBufSize;
        return m_iHeadPos != m_iTailPos;
    }

    public int GetBufferSize()
    {
        return m_iBufSize;
    }

    public int GetHeadPos()
    {
        return m_iHeadPos;
    }

    public int GetTailPos()
    {
        return m_iTailPos;
    }

    public bool GetCompletedPacket(byte[] tempBuffer, ref int packetLength, ref int packetNo)
    {
		// 현재 길이가 4 미만이면 끝 ( 패킷길이2 + 패킷번호2 )
		int validLen = GetValidCount();
		if( validLen < 4 )
		{
			return false;
		}

        GetData(m_pTempBuffer, 2);
        int packetLen = BitConverter.ToInt16(m_pTempBuffer, 0);
        if ( validLen >= (packetLen + 4 ) )
		{
			//패킷 완성
            packetLength = packetLen;
			HeadIncrease(2);

            GetData(m_pTempBuffer, 2);
            packetNo = BitConverter.ToInt16(m_pTempBuffer, 0);
            HeadIncrease(2);

			GetData( tempBuffer , packetLen );
			HeadIncrease(packetLen );
			return true;
		}
		else
		{
			return false;
		}
    }

    /*
    public int GetNoFrom5Byte(byte[] tempBuffer)
    {
        //byte[] -> string -> int
        string strData = Encoding.Default.GetString(tempBuffer);
        string strNo = strData.Substring(0, 5);
        int dataNo = 0;
        int.TryParse(strNo, out dataNo);
        return dataNo;
    }

    public int GetNoFrom10Byte(byte[] tempBuffer)
    {
        //byte[] -> string -> int
        string strData = Encoding.Default.GetString(tempBuffer);
        string strNo = strData.Substring(0, 10);
        int dataNo = 0;
        int.TryParse(strNo, out dataNo);
        return dataNo;
    }
    */

}