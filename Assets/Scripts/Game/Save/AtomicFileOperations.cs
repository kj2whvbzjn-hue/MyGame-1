using System;
using System.IO;

namespace GuildAdventure.Game.Save
{
    public interface IAtomicFileOperations
    {
        bool Exists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string content);
        void Copy(string source, string destination, bool overwrite);
        void Delete(string path);
        void Move(string source, string destination);
        void CreateDirectory(string path);
    }

    public sealed class SystemAtomicFileOperations : IAtomicFileOperations
    {
        public bool Exists(string path) => File.Exists(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
        public void Copy(string source, string destination, bool overwrite) => File.Copy(source, destination, overwrite);
        public void Delete(string path) => File.Delete(path);
        public void Move(string source, string destination) => File.Move(source, destination);
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    }

    public sealed class FaultInjectingFileOperations : IAtomicFileOperations
    {
        readonly IAtomicFileOperations inner;
        public string FailOperation;
        public int FailureCount { get; private set; }

        public FaultInjectingFileOperations(IAtomicFileOperations inner)
        {
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        void Check(string operation)
        {
            if (!string.Equals(FailOperation, operation, StringComparison.Ordinal)) return;
            FailOperation = null;
            FailureCount++;
            throw new IOException("injected_" + operation + "_failure");
        }

        public bool Exists(string path) => inner.Exists(path);
        public string ReadAllText(string path) { Check("read"); return inner.ReadAllText(path); }
        public void WriteAllText(string path, string content) { Check("write"); inner.WriteAllText(path, content); }
        public void Copy(string source, string destination, bool overwrite) { Check("backup"); inner.Copy(source, destination, overwrite); }
        public void Delete(string path) { Check("delete"); inner.Delete(path); }
        public void Move(string source, string destination) { Check("commit"); inner.Move(source, destination); }
        public void CreateDirectory(string path) => inner.CreateDirectory(path);
    }
}
