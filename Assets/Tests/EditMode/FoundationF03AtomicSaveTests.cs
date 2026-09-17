#if UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NUnit.Framework;
using GuildAdventure.Game.Save;

namespace GuildAdventure.Tests.EditMode
{
    public sealed class FoundationF03AtomicSaveTests
    {
        string dir;
        string path;

        [SetUp]
        public void SetUp()
        {
            dir=Path.Combine(Path.GetTempPath(),"guild-adventure-f03-"+Guid.NewGuid().ToString("N"));
            path=Path.Combine(dir,"save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if(Directory.Exists(dir))Directory.Delete(dir,true);
        }

        static GameSaveState State(string id)
        {
            return new GameSaveState{saveId=id};
        }

        [Test]
        public void BackupFailure_PreservesPreviousGoodSave()
        {
            var system=new SystemAtomicFileOperations();
            var store=new JsonFileGameSaveStore(path,system);
            store.Write(State("old"));

            var faults=new FaultInjectingFileOperations(system){FailOperation="backup"};
            Assert.Throws<IOException>(()=>new JsonFileGameSaveStore(path,faults).Write(State("new")));

            Assert.AreEqual("old",store.Load().saveId);
        }

        [Test]
        public void CommitFailure_RestoresPreviousGoodSave()
        {
            var system=new SystemAtomicFileOperations();
            var store=new JsonFileGameSaveStore(path,system);
            store.Write(State("old"));

            var faults=new FaultInjectingFileOperations(system){FailOperation="commit"};
            Assert.Throws<IOException>(()=>new JsonFileGameSaveStore(path,faults).Write(State("new")));

            Assert.AreEqual("old",store.Load().saveId);
            Assert.IsTrue(File.Exists(path));
        }

        [Test]
        public void CandidateWriteFailure_PreservesPreviousGoodSave()
        {
            var system=new SystemAtomicFileOperations();
            var store=new JsonFileGameSaveStore(path,system);
            store.Write(State("old"));

            var faults=new FaultInjectingFileOperations(system){FailOperation="write"};
            Assert.Throws<IOException>(()=>new JsonFileGameSaveStore(path,faults).Write(State("new")));

            Assert.AreEqual("old",store.Load().saveId);
        }
    }
}
#endif
