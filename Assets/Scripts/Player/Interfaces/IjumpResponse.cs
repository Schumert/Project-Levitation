

public interface IJumpResponse
{
    /// <summary>
    /// Zıplama isteğini işleyip gerçekten zıpladıysa true döner.
    /// </summary>
    /// <param name="jumpPressed">Bu frame zıplama tuşuna basıldı mı?</param>
    bool TryJump(bool jumpPressed);

    /// <summary>
    /// Şu anda yerde mi?
    /// </summary>
    bool isGrounded { get; }
}