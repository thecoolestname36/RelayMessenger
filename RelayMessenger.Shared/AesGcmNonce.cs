namespace RelayMessenger.Shared;

public class AesGcmNonce
{
    private int _salt = 0;
    private readonly bool _saltDirection;
    private readonly string _nonceExplicitSource;
    
    public const sbyte Length = 12;
        
    /// <param name="saltDirection">True for addition, false for subtraction</param>
    public AesGcmNonce(bool saltDirection)
    {
        _saltDirection = saltDirection;
        _nonceExplicitSource = Guid.NewGuid().ToString("N");
    }

    public byte[] GenerateNonce()
    {
        _salt = _saltDirection ? _salt++ : _salt--;
        var nonce = new byte[Length];
        Buffer.BlockCopy(BitConverter.GetBytes(_salt), 0, nonce, 0, 4);
        var nonceExplicit = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        for (byte i = 0; i < 8; i++)
        {
            nonceExplicit[i] = (byte)(nonceExplicit[i] ^ (byte)_nonceExplicitSource[i]);
        }
        Buffer.BlockCopy(nonceExplicit, 0, nonce, 4, 8);
        return nonce;
    }
}

public class AesGcmTag
{
    public const sbyte Length = 16; // MaxSize = 16
}
