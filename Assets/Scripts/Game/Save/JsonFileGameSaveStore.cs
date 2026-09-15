using System;
using System.IO;
using UnityEngine;

namespace GuildAdventure.Game.Save
{
    public sealed class JsonFileGameSaveStore : ISaveStore<GameSaveState>
    {
        readonly string path;
        readonly string backupPath;

        public JsonFileGameSaveStore(string path)
        {
            if(string.IsNullOrWhiteSpace(path))throw new ArgumentException(nameof(path));
            this.path=path; backupPath=path+".bak";
        }

        public GameSaveState Load()
        {
            if(!File.Exists(path))
            {
                if(!File.Exists(backupPath))return null;
                return Deserialize(File.ReadAllText(backupPath));
            }
            try { return Deserialize(File.ReadAllText(path)); }
            catch
            {
                if(!File.Exists(backupPath))throw;
                return Deserialize(File.ReadAllText(backupPath));
            }
        }

        public void Write(GameSaveState value)
        {
            var error=GameSaveState.Validate(value);
            if(!string.IsNullOrEmpty(error))throw new InvalidDataException(error);
            var json=JsonUtility.ToJson(value,true);
            var dir=Path.GetDirectoryName(path);if(!string.IsNullOrEmpty(dir))Directory.CreateDirectory(dir);
            var temp=path+".tmp";
            File.WriteAllText(temp,json);
            // Validate serialized candidate before touching the previous good save.
            var reread=Deserialize(File.ReadAllText(temp));
            var rereadError=GameSaveState.Validate(reread);
            if(!string.IsNullOrEmpty(rereadError)){File.Delete(temp);throw new InvalidDataException(rereadError);}
            if(File.Exists(path))File.Copy(path,backupPath,true);
            if(File.Exists(path))File.Delete(path);
            File.Move(temp,path);
        }

        static GameSaveState Deserialize(string json)
        {
            var s=JsonUtility.FromJson<GameSaveState>(json);
            var e=GameSaveState.Validate(s);
            if(!string.IsNullOrEmpty(e))throw new InvalidDataException(e);
            return s;
        }
    }
}
