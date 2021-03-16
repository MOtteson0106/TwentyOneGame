using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Casino;
using Casino.TwentyOne;
using System.Data.SqlClient;
using System.Data;

namespace TwentyOne//Every data type in C# is either a reference type or a value type
{
    class Program
    {
        private static object sqlDbType;

        static void Main(string[] args)
        {
            //string text = File.ReadAllText(@"C:\Users\Student\Logs\log.txt");
            //DateTime yearOfBirth = new DateTime(1995, 5, 23, 8, 23, 45);
            //DateTime yearOfGraduation = new DateTime(2015, 6, 6, 10, 30, 20);
            //TimeSpan ageAtGraduation = yearOfGraduation - yearOfBirth;
            //Console.WriteLine(ageAtGraduation.Days / 365);
            ////TwentyOneGame game = new TwentyOneGame();
            ////game.Players = new List<string>() { "June", "Hannah", "Molly" };
            ////game.ListPlayers();
            ////Console.ReadLine();
            ////Inheritance is one of the three pillars of Object Oriented Programming
            ////PolyMorphism is the second pillar of Object Oriented Programming
            ////TwentyOneGame game = new TwentyOneGame();
            ////List<Game> games = new List<Game>();
            //Game game = new TwentyOneGame(); //Twenty One game inherits from Game
            ////game.Players = new List<Player>() { "Matt", "Hannah", "June" };
            //game.Players = new List<Player>();
            //Player player = new Player();
            //player.Name = "Matt";
            //game += player;
            //game -= player;
            ////game.ListPlayers();
            //Deck deck = new Deck();
            //int count = deck.Cards.Count(x => x.Face == Face.Ace);
            //List<Card> newList = deck.Cards.Where(x => x.Face == Face.King).ToList();
            //List<int> numberList = new List<int>() { 1, 44, 55, 678, 93045, 342 };
            ////int sum = numberList.Sum(x => x + 5);
            //int sum = numberList.Where(x => x > 20).Sum();
            //Console.WriteLine(sum);
            //Card card1 = new Card();
            //Card card2 = card1;
            //card1.Face = Face.Eight;
            //card2.Face = Face.King;
            //Console.WriteLine(card1);
            ////Card card = new Card();
            ////card.Suit = Suit.Clubs;
            ////int underlyingvalue = (int)Suit.Diamonds;
            ////Console.WriteLine(underlyingvalue);
            ////Card card = new Card() { Face = "King", Suit = "Spades" }; 
            ////method above initializes values easier than card.Face = "King" & card.Suit = "Spades"
            ////You group it into one phrase.
            ////deck.Shuffle();
            //foreach (Card card in deck.Cards)
            //{
            //    Console.WriteLine(card.Face + " of " + card.Suit);
            //}
            //Console.WriteLine(deck.Cards.Count);
            //Console.ReadLine();





            ////public static Deck Shuffle(Deck deck, int times)
            ////{
            ////    for (int i = 0; i < times; i++)
            ////    {
            ////        deck = Shuffle(deck);
            ////    }
            ////    return deck;
            ///
            Guid identifier = Guid.NewGuid();//Global Unique Identifier, value: constant data.
            const string casinoName = "KIOT's Hotel and Casino";
            Console.WriteLine("Welcome to {0}! \n " +
                "Enter your name please: ",casinoName);
            string playerName = Console.ReadLine();
            if (playerName.ToLower() == "admin")
            {
                List<ExceptionEntity> Exceptions = ReadExceptions();
                foreach(var exception in Exceptions)
                {
                    Console.Write(exception.Id + "|");
                    Console.Write(exception.ExceptionType + "|");
                    Console.Write(exception.ExceptionMessage + "|");
                    Console.Write(exception.TimeStamp);
                    Console.WriteLine();
                    Console.ReadLine();
                }
                Console.ReadLine();
                return;
            }
            bool validAnswer = false;
            int bank = 0;
            while (!validAnswer)
            {
                Console.WriteLine("Enter your total cash:");
                validAnswer = int.TryParse(Console.ReadLine(), out bank);
                if (!validAnswer) Console.WriteLine("Please enter digits only, omit decimal points.");
            }
            Console.WriteLine("Hello, {0}. Would you like to join a game of 21 right now? ", playerName);
            string answer = Console.ReadLine().ToLower();
            if (answer == "yes" || answer == "yeah" || answer == "y" || answer == "ya" || answer == "sure")
            {
                Player player = new Player(playerName, bank);
                player.ID = Guid.NewGuid();
                using(StreamWriter file = new StreamWriter(@"C:\Users\Student\Logs\log.txt", true))
                {
                    file.WriteLine(player.ID);
                }
                Game game = new TwentyOneGame();
                game += player;
                player.isPlaying = true;
                while (player.isPlaying && player.Balance > 0)
                {
                    try
                    {
                        game.Play();
                    }
                    catch (FraudException ex)
                    {
                        Console.WriteLine("A security violation has occurred.\n Remain seated while the cops come to beat yo' ass!");
                        UpdateDatabaseWithExceptions(ex);
                        Console.ReadLine();
                        return; 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred.");
                        UpdateDatabaseWithExceptions(ex);
                        Console.ReadLine();
                        return;
                    }
                }
                game -= player;
                Console.WriteLine("Thank you for playing. ");
            }
            Console.WriteLine("Feel free to look around the casino, bye for now.");
            Console.Read();

        }
        private static void UpdateDatabaseWithExceptions(Exception ex)
        {
            string connectionString = @"Data Source = (localdb)\ProjectsV13; 
                                        Initial Catalog = TwentyOneGame; 
                                        Integrated Security = True; Connect Timeout = 30; 
                                        Encrypt = False; TrustServerCertificate = False; 
                                        ApplicationIntent = ReadWrite; 
                                        MultiSubnetFailover = False";
            string queryString = @"INSERT INTO Exceptions (ExceptionType, ExceptionMessage, TimeStamp) VALUES
                                    (@ExceptionType, @ExceptionMessage, @TimeStamp)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.Add("@ExceptionType", SqlDbType.VarChar);
                command.Parameters.Add("@ExceptionMessage", SqlDbType.VarChar);
                command.Parameters.Add("@TimeStamp", SqlDbType.DateTime);

                command.Parameters["@ExceptionType"].Value = ex.GetType().ToString();
                command.Parameters["@ExceptionMessage"].Value = ex.Message;
                command.Parameters["@TimeStamp"].Value = DateTime.Now;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
        private static List<ExceptionEntity> ReadExceptions()
        {
            string connectionString = @"Data Source = (localdb)\ProjectsV13; 
                                        Initial Catalog = TwentyOneGame; 
                                        Integrated Security = True; Connect Timeout = 30; 
                                        Encrypt = False; TrustServerCertificate = False; 
                                        ApplicationIntent = ReadWrite; 
                                        MultiSubnetFailover = False";
            string queryString = @"Select Id, ExceptionType, ExceptionMessage, TimeStamp From Exceptions";
            List<ExceptionEntity> Exceptions = new List<ExceptionEntity>();
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ExceptionEntity exception = new ExceptionEntity();
                    exception.Id = Convert.ToInt32(reader["Id"]);
                    exception.ExceptionType = reader["ExceptionType"].ToString();
                    exception.ExceptionMessage = reader["ExceptionMessage"].ToString();
                    exception.TimeStamp = Convert.ToDateTime(reader["TimeStamp"]);
                    Exceptions.Add(exception);
                }
                connection.Close();
            }

            return Exceptions;
        }
    }
}
