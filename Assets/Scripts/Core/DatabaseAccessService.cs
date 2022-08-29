using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DatabaseAccessService : MonoBehaviour
{
    private MongoClient _mongoClient = new MongoClient("mongodb+srv://DerTraeumer:Isqeqbepiscoke1070@cluster0.8i2lysa.mongodb.net/?retryWrites=true&w=majority");
    IMongoDatabase _database;
    IMongoCollection<BsonDocument> _collection;


    private void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
        _database = _mongoClient.GetDatabase("SPODB");

        _collection = _database.GetCollection<BsonDocument>("UsersData");
    }

    public async void SaveNewData(UserDataPack data)
    {
        var document = new BsonDocument { { "Id", data.Id}, { "Nickname" , data.Nickname} , { "Experience" , data.Experience}, { "Level" , data.Level} };
        await _collection.InsertOneAsync(document);
    }

    public void UpdateExperience(UserDataPack data)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("Id", data.Id);
        var update = Builders<BsonDocument>.Update.Set("Experience", data.Experience);
        _collection.UpdateOneAsync(filter, update);
    }

    public async Task<List<UserDataPack>> GetDataFromDB()
    {
        var allDatasTask = _collection.FindAsync(new BsonDocument());
        var datasAwaied = await allDatasTask;

        List<UserDataPack> datas = new List<UserDataPack>();
        foreach(var data in datasAwaied.ToList())
        {
            datas.Add(Deserialize(data.ToString()));
        }
        return datas;
    }

    private UserDataPack Deserialize(string v)
    {
        var data = new UserDataPack();
        var stringWithoutId = v.Substring(v.IndexOf("),") + 4); //"Id\" : \"the boss\", \"Nickname\" : \"Roman\", \"Experience\" : 99999, \"Level\" : 10000 }"
        var kvps = stringWithoutId.Split(',');
        var id = kvps[0].Substring(kvps[0].IndexOf(":")+3, kvps[0].Substring(kvps[0].IndexOf(":") + 3).Length-1);
        var nick = kvps[1].Substring(kvps[1].IndexOf(":") + 3, kvps[1].Substring(kvps[1].IndexOf(":") + 3).Length - 1);
        var exp = int.Parse(kvps[2].Substring(kvps[2].IndexOf(":")+1, kvps[2].Substring(kvps[2].IndexOf(":")+1).Length));
        var lvl = int.Parse(kvps[3].Substring(kvps[3].IndexOf(":") + 1, kvps[3].Substring(kvps[3].IndexOf(":") + 1).Length-1));
        data.Id = id;
        data.Nickname = nick;
        data.Experience = exp;
        data.Level = lvl;
        return data;
    }
}
[Serializable]
public class UserDataPack
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id;
    public string Nickname;
    public int Experience;
    public int Level;
}