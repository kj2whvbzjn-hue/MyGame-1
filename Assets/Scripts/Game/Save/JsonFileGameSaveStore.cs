using System;
using System.IO;
using UnityEngine;

namespace GuildAdventure.Game.Save
{
    public sealed class JsonFileGameSaveStore : ISaveStore<GameSaveState>
    {
        readonly string path;
        readonly string backupPath;
        readonly IAtomicFileOperations files;

        public JsonFileGameSaveStore(string path) : this(path, new SystemAtomicFileOperations()) { }

        public JsonFileGameSaveStore(string path, IAtomicFileOperations files)
        {
            if(string.IsNullOrWhiteSpace(path))throw new ArgumentException(nameof(path));
            this.files=files??throw new ArgumentNullException(nameof(files));
            this.path=path; backupPath=path+".bak";
        }

        public GameSaveState Load()
        {
            if(!files.Exists(path))
            {
                if(!files.Exists(backupPath))return null;
                return Deserialize(files.ReadAllText(backupPath));
            }
            try { return Deserialize(files.ReadAllText(path)); }
            catch
            {
                if(!files.Exists(backupPath))throw;
                return Deserialize(files.ReadAllText(backupPath));
            }
        }

        public void Write(GameSaveState value)
        {
            var error=GameSaveState.Validate(value);
            if(!string.IsNullOrEmpty(error))throw new InvalidDataException(error);
            var json=JsonUtility.ToJson(value,true);
            var dir=Path.GetDirectoryName(path);if(!string.IsNullOrEmpty(dir))files.CreateDirectory(dir);
            var temp=path+".tmp";
            files.WriteAllText(temp,json);
            try
            {
                // Validate serialized candidate before touching the previous good save.
                var reread=Deserialize(files.ReadAllText(temp));
                var rereadError=GameSaveState.Validate(reread);
                if(!string.IsNullOrEmpty(rereadError))throw new InvalidDataException(rereadError);

                if(files.Exists(path))files.Copy(path,backupPath,true);
                if(files.Exists(path))files.Delete(path);
                try { files.Move(temp,path); }
                catch
                {
                    // A commit failure must leave a readable previous-good save at the primary path.
                    if(!files.Exists(path)&&files.Exists(backupPath))files.Copy(backupPath,path,true);
                    throw;
                }
            }
            finally
            {
                if(files.Exists(temp))files.Delete(temp);
            }
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
