# T8-GSC-DECOMPILER

This was my attempt to create a GSC decompiler for BO4 , there are a lot of opcodes missing. 
This project was abandoned after the release of the Decompiled T8 script.

This is the 32 bit Hash Algorithm reversed from the game which is written in c#. 
Be careful if you want to convert it to another language pay attention to all cast.

```c#
public static uint _32BitHash(string input)
{
    string Entry = input;
    int HashKey = 0x4b9ace2f;

    for (int i = 0; i < Entry.Length; i++)
     {
         int Value = 0x19 < (byte)(Entry[i] + 0XBF) ? Entry[i] : (Entry[i] + 0x20);
         uint val2 = (uint)(((Value + HashKey) * 0x400) ^ (Value + HashKey));
         // on update la key : 
         HashKey = (int)((val2 >> 6) + val2);
     }
     uint result = (uint)((((uint)(HashKey * 9) >> 0xb ^ HashKey * 9) * 0x8001));
     return result;
}
```
## Credit:
* Seriousyt
* Scobalula 
