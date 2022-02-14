namespace UnitTest;

public class TestMessage : IDeserializable<TestMessage>, ISerializable
{
    public TestMessage()
    { }

    public TestMessage(String msg)
    {
        this.Message = msg;
    }

    public String Message 
    { 
        get; 
        set; 
    } = String.Empty;

    [return: NotNull]
    public static TestMessage ConstructFromSerializationData([DisallowNull] ISerializationInfoGetter info)
    {
        String msg = info.GetState<String>(nameof(Message))!;
        return new(msg: msg);
    }

    public void GetSerializationData([DisallowNull] ISerializationInfoAdder info) =>
        info.AddState(nameof(this.Message),
                      this.Message);
}