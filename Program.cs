using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using jfObsidian;

namespace noteCardGen;

public class Program
{

    public static void Main(string[] args)
    {

        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("note_card_generator_csharp {root_note}");
            Console.WriteLine("note_card_generator_csharp {root_note} {blacklisted_notes}");
            return;
        }

        List<string> blacklisted_keywords = new List<string>();

        if (args.Length > 1)
        {
            for (int i = 1; i < args.Length; i++)
            {
                blacklisted_keywords.Add(args[i]);
            }

        }

        Console.WriteLine("Loading {0}'s link tree", args[0]);
        StreamWriter notes_chain_log_file = new StreamWriter(args[0] + "_chain.txt");
        HashSet<string> set = new HashSet<string>();
        var root_note = new NoteNode(args[0], set, null, notes_chain_log_file, blacklisted_keywords);

        Console.WriteLine(root_note.noteCardTreeSize() + " notes loaded.");
        Console.WriteLine("generating definitions...");

        root_note.generateCards();
        string path = args[0] + "_quizlet.txt";

        StreamWriter output_file = new StreamWriter(path);
        root_note.writeCards(output_file);

        output_file.Close();
        notes_chain_log_file.Close();

        Console.WriteLine("note cards generated...\ngenerating blank notes list");
        root_note.GenerateBlankNotesListFile();
    }
}