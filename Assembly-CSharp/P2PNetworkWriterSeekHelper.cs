using System;

public struct P2PNetworkWriterSeekHelper : IDisposable
{
	public P2PNetworkWriterSeekHelper(P2PNetworkWriter writer)
	{
		this.m_Writer = writer;
		this.m_StoredPos = (int)this.m_Writer.Position;
		this.m_SeekFromPos = this.m_StoredPos;
	}

	public void StoreCurrentPos()
	{
		this.m_StoredPos = (int)this.m_Writer.Position;
	}

	public void SeekToStoredPos()
	{
		this.m_SeekFromPos = (int)this.m_Writer.Position;
		this.m_Writer.Seek(this.m_StoredPos - (int)this.m_Writer.Position);
	}

	public void Dispose()
	{
		this.m_Writer.Seek(this.m_SeekFromPos - (int)this.m_Writer.Position);
		DebugUtils.Assert(this.m_SeekFromPos == (int)this.m_Writer.Position, true);
	}

	private P2PNetworkWriter m_Writer;

	private int m_StoredPos;

	private int m_SeekFromPos;
}
