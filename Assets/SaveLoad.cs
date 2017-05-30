using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class GameData
{
    public static string playerName = "I dont know if we need this bit";
    public static int playerLevel = 1;
}
//function saves two variables from this class as a test, then tries to save LevelStructure.levelStructures and fails presently.
//the two error messages are:
/**
 * on loading:
EndOfStreamException: Failed to read past end of stream.
System.IO.BinaryReader.ReadByte () (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.IO/BinaryReader.cs:293)
System.Runtime.Serialization.Formatters.Binary.ObjectReader.ReadNextObject (System.IO.BinaryReader reader) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectReader.cs:142)
System.Runtime.Serialization.Formatters.Binary.ObjectReader.ReadObjectGraph (BinaryElement elem, System.IO.BinaryReader reader, Boolean readHeaders, System.Object& result, System.Runtime.Remoting.Messaging.Header[]& headers) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectReader.cs:110)
System.Runtime.Serialization.Formatters.Binary.BinaryFormatter.NoCheckDeserialize (System.IO.Stream serializationStream, System.Runtime.Remoting.Messaging.HeaderHandler handler) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/BinaryFormatter.cs:179)
System.Runtime.Serialization.Formatters.Binary.BinaryFormatter.Deserialize (System.IO.Stream serializationStream) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/BinaryFormatter.cs:136)
SaveLoad.Load () (at Assets/Scripts/SaveLoad.cs:43)
Manager.Start () (at Assets/Scripts/Manager.cs:10)
 * 
 * on saving:
 * SerializationException: Type UnityEngine.Networking.MessageBase in assembly UnityEngine.Networking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null is not marked as serializable.
System.Runtime.Serialization.FormatterServices.GetSerializableMembers (System.Type type, StreamingContext context) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization/FormatterServices.cs:101)
System.Runtime.Serialization.Formatters.Binary.CodeGenerator.GenerateMetadataTypeInternal (System.Type type, StreamingContext context) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/CodeGenerator.cs:78)
System.Runtime.Serialization.Formatters.Binary.CodeGenerator.GenerateMetadataType (System.Type type, StreamingContext context) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/CodeGenerator.cs:64)
System.Runtime.Serialization.Formatters.Binary.ObjectWriter.CreateMemberTypeMetadata (System.Type type) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectWriter.cs:442)
System.Runtime.Serialization.Formatters.Binary.ObjectWriter.GetObjectData (System.Object obj, System.Runtime.Serialization.Formatters.Binary.TypeMetadata& metadata, System.Object& data) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectWriter.cs:430)
System.Runtime.Serialization.Formatters.Binary.ObjectWriter.WriteObject (System.IO.BinaryWriter writer, Int64 id, System.Object obj) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectWriter.cs:306)
System.Runtime.Serialization.Formatters.Binary.ObjectWriter.WriteObjectInstance (System.IO.BinaryWriter writer, System.Object obj, Boolean isValueObject) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectWriter.cs:293)
System.Runtime.Serialization.Formatters.Binary.ObjectWriter.WriteQueuedObjects (System.IO.BinaryWriter writer) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectWriter.cs:271)
System.Runtime.Serialization.Formatters.Binary.ObjectWriter.WriteObjectGraph (System.IO.BinaryWriter writer, System.Object obj, System.Runtime.Remoting.Messaging.Header[] headers) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/ObjectWriter.cs:256)
System.Runtime.Serialization.Formatters.Binary.BinaryFormatter.Serialize (System.IO.Stream serializationStream, System.Object graph, System.Runtime.Remoting.Messaging.Header[] headers) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/BinaryFormatter.cs:232)
System.Runtime.Serialization.Formatters.Binary.BinaryFormatter.Serialize (System.IO.Stream serializationStream, System.Object graph) (at /Users/builduser/buildslave/mono/build/mcs/class/corlib/System.Runtime.Serialization.Formatters.Binary/BinaryFormatter.cs:211)
SaveLoad.Save () (at Assets/Scripts/SaveLoad.cs:29)
Manager.Update () (at Assets/Scripts/Manager.cs:23)
 * 
 * 
 * These only cropped up when I finally was able to *attempt* to save level structure.
 *  
 */
public class SaveLoad
{
    public static void Save()//save function
    {

        
        //converts values into binary
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (FileStream fs = new FileStream("gamesave.bin", FileMode.Create, FileAccess.Write))
        {
            binaryFormatter.Serialize(fs, GameData.playerName);
            binaryFormatter.Serialize(fs, GameData.playerLevel);
            binaryFormatter.Serialize(fs, LevelStructure.levelStructures);
            //binaryFormatter.Serialize(fs, ClientDetails.position);
        }
    }

    public static void Load()//load funciton
    {
        if (!File.Exists("gamesave.bin"))//if the file exists, load it
        { return; }
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (FileStream fs = new FileStream("gamesave.bin", FileMode.Open, FileAccess.Read))
        {
            GameData.playerName = (string)binaryFormatter.Deserialize(fs);
            GameData.playerLevel = (int)binaryFormatter.Deserialize(fs);
            LevelStructure.levelStructures = (RegionBlock [,])binaryFormatter.Deserialize(fs);
        }
    }
}
