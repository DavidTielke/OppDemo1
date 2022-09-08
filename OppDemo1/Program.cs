using Ninject;

namespace OppDemo1
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // A1: Person anlegen
            var person = new Person
            {
                Id = 0,
                Name = "David",
                Age = 38
            };

            var kernel = new StandardKernel();

            // Fuck-Up-Point
            kernel.Bind<IPersonManager>().To<PersonManager>();
            kernel.Bind<IPersonRepository>().To<PersonRepository>();
            kernel.Bind<IPersonSerializer>().To<PersonCsvSerializer>();
            kernel.Bind<IStringStorer>().To<StringDbStorer>();

            var manager = kernel.Get<IPersonManager>();

            manager.Add(person);
        }
    }

    public interface IPersonManager
    {
        void Add(Person person);
    }

    public class PersonManager : IPersonManager
    {
        private readonly IPersonRepository _repository;

        public PersonManager(IPersonRepository repository)
        {
            _repository = repository;
        }

        public void Add(Person person)
        {
            if (person.Id != 0 || person.Age > 99 || person.Age < 1)
            {
                throw new ArgumentException("Person is not valid", nameof(person));
            }

            _repository.Insert(person);
        }
    }

    public interface IPersonRepository
    {
        void Insert(Person person);
    }

    public class PersonRepository : IPersonRepository
    {
        private readonly IPersonSerializer _serializer;
        private readonly IStringStorer _storer;

        public PersonRepository(IPersonSerializer serializer, IStringStorer storer)
        {
            _serializer = serializer;
            _storer = storer;
        }

        public void Insert(Person person)
        {
            // Person serialisieren
            var csvPerson = _serializer.Serialize(person);

            // Person persistieren
            _storer.Save("data.csv", csvPerson);
        }
    }

    public interface IPersonSerializer
    {
        string Serialize(Person person);
    }

    public class PersonCsvSerializer : IPersonSerializer
    {
        public string Serialize(Person person)
        {
            var csvPerson = $"{person.Id},{person.Name},{person.Age}";
            return csvPerson;
        }
    }

    public interface IStringStorer
    {
        void Save(string path, string data);
    }

    public class StringDbStorer : IStringStorer
    {
        public void Save(string path, string data)
        {
            Console.WriteLine("Objekt in die Datenbank geschrieben");
            // ...
        }
    }

    public class StringFileStorer : IStringStorer
    {
        public void Save(string path, string data)
        {
            Console.WriteLine("Objekt in Dateisystem geschrieben");
            File.WriteAllText(path, data);
        }
    }
}