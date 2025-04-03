using System.Diagnostics.Metrics;
namespace Akvelon_Coding_First_task
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FizzBuzzDetector FizzBuzzDetector = new();
            string msg = "Mary had a little lamb Little lamb, little lamb Mary had a little lamb It's fleece was white as snow";
            var count = FizzBuzzDetector.getOverlappings(ref msg);
            Console.WriteLine(msg);
            Console.WriteLine(count);

        }
    }
    public class FizzBuzzDetector
    {
        // Private Field Hold the Count of Decorated Words
        private int _fizzBuzzDetectorCount = 0;
        // Private Field Hold the New Or the Decorated String
        private string DecoratedMessage;

        /// <summary>
        ///     getOverlappings Method Used To Decorate Specific Message With this Constraint 
        ///      (replaces every third word in the string to Fizz, and every fifth word in the string to Buzz.)
        ///     
        /// </summary>
        /// <param name="msg"> 
        ///      Reference Parameter Used To Change the Message to the Decorated Message
        /// </param>
        /// <returns> Returns Count of the Decorated (Changed) Words</returns>
        public int getOverlappings(ref string msg)
        {
            var Words = msg.Split(" ");
            for (int i = 0; i < Words.Length ; i++)
            {
                // Used To Check the Special Characters bt ASCII Code   
                if (Words[i].Length == 1 && 
                    ( ((int)Words[i].ToCharArray().First() >=32  && (int)Words[i].ToCharArray().First() <= 47)
                    ||((int)Words[i].ToCharArray().First() >= 58 && (int)Words[i].ToCharArray().First() <= 64)
                    ||((int)Words[i].ToCharArray().First() >= 91 && (int)Words[i].ToCharArray().First() <= 96)
                    ||((int)Words[i].ToCharArray().First() >= 123 && (int)Words[i].ToCharArray().First() <= 126)))
                {
                    Words[i] = Words[i];
                }
                // if Not Special Character Apply the Logic
                else
                {
                    if ((i+1) % 3 == 0 && (i + 1) % 5 != 0)
                    {
                        Words[i] = "Fizz";
                        _fizzBuzzDetectorCount++;
                    }
                    else if ((i+1) % 5 == 0 && (i + 1) % 3 != 0)
                    {

                        Words[i] = "Buzz";
                        _fizzBuzzDetectorCount++;
                    }
                    else if ((i+1) % 3 == 0 && (i+1) % 5 == 0)
                    {
                        Words[i] = "FizzBuzz";
                        _fizzBuzzDetectorCount++;
                    }
                }
            }
            // Generate the Decorated Message
            for (int i = 0; i < Words.Length; i++)
            {
                DecoratedMessage += $"{Words[i]}";
                if (i != Words.Length - 1)
                    DecoratedMessage += " ";
            }
            msg = DecoratedMessage;
            return _fizzBuzzDetectorCount;
        }
    }
}
