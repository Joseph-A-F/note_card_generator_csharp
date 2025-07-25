using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace note_card_gen;

public class Program
{

    public static void Main(string[] args)
    {
#if DEBUG
        Directory.SetCurrentDirectory("/Users/nausetjf/Documents/ObsidianFiles/NausetVault/NausetVault_SyncThing");

#endif
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("note_card_generator_csharp {root_note}");
            Console.WriteLine("note_card_generator_csharp {root_note} {blacklisted_notes}");
            // Console.WriteLine("note_card_generator_csharp {root_note} -c {blacklisted_notes}");
            // Console.WriteLine("-c copy to clipboard");
            return;
        }
        List<string> blacklisted_keywords = new List<string>();
        // bool copy_to_clipboard = false;
        if (args.Length > 1)
        {
            // int when_blacklisted_notes_start = 1;
            // if (args[1] == "-c")
            // {
            // copy_to_clipboard = true;
            // }

            for (int i = 1; i < args.Length; i++)
            {
                blacklisted_keywords.Add(args[i]);
            }

        }
        Console.WriteLine("Loading {0}'s link tree", args[0]);
        StreamWriter notes_chain_log_file = new StreamWriter(args[0] + "_chain.txt");
        HashSet<string> set = new HashSet<string>();
        var root_note = new NoteNode(args[0], set, null, notes_chain_log_file, blacklisted_keywords);

        Console.WriteLine(root_note.note_card_tree_size() + " notes loaded.");
        Console.WriteLine("generating definitions...");

        root_note.generate_cards();
        string path = args[0] + "_quizlet.txt";

        StreamWriter output_file = new StreamWriter(path);
        root_note.write_cards(output_file);

        output_file.Close();
        notes_chain_log_file.Close();

        Console.WriteLine("note cards generated...\ngenerating blank notes list");
        root_note.generate_blank_notes_list_file();
    }
}

public class NoteNode
{
    public string name;
    public string filepath;
    public string real_file_contents;
    public string note_card_contents;
    public List<NoteNode> links;
    public NoteNode parent;

    // private string v;
    private HashSet<string> set;
    private string v;
    private StreamWriter log_file;
    private List<string> blacklist;
    // private List<string> blacklisted_keywords;

    public NoteNode(string linkname, NoteNode parent = null)
    {

        this.name = linkname;
        this.filepath = get_filepath_from_name(linkname);
        // var stream = File.OpenRead(filepath);

        if (this.filepath == "") this.real_file_contents = "";
        else this.real_file_contents = File.ReadAllText(filepath);

        this.note_card_contents = "";
        // this.parent = parent;
        this.links = get_links();



    }

    public NoteNode(string linkname, HashSet<string> set, NoteNode parent = null, StreamWriter notes_chain_log_file = null, List<string> blacklist = null)
    {
        this.name = linkname;
        this.filepath = get_filepath_from_name(linkname);
        // var stream = File.OpenRead(filepath);
        this.parent = parent;
        this.log_file = notes_chain_log_file;
        this.blacklist = blacklist;
        print_parent_link_chain();
        // if (this.filepath == "") System.Console.WriteLine("cannot find {0}", linkname);
        if (this.filepath == "") this.real_file_contents = "";
        else this.real_file_contents = File.ReadAllText(filepath);

        this.note_card_contents = "";
        this.set = set;
        set.Add(this.name);
        this.links = get_links();

    }

    private void print_parent_link_chain()
    {
        NoteNode temp = this.parent;
        string line = "->" + name;
        while (temp != null)
        {
            if (temp.parent == null) line = temp.name + line;
            else line = "->" + temp.name + line;
            temp = temp.parent;
        }
        System.Console.WriteLine(line);
        if (log_file != null)
        {
            log_file.WriteLine(line);
        }
        // throw new NotImplementedException();
    }

    public NoteNode()
    {
    }

    // public NoteNode(string linkname, HashSet<string> set, NoteNode parent = null, StreamWriter notes_chain_log_file = null, List<string> blacklisted_keywords = null) : this(linkname, set, parent, notes_chain_log_file)
    // {
    // this.blacklisted_keywords = blacklisted_keywords;
    // }

    // public NoteNode(string v, HashSet<string> set, StreamWriter notes_chain_log_file)
    // {
    //     this.v = v;
    //     this.set = set;
    //     this.notes_chain_log_file = notes_chain_log_file;
    // }

    public List<NoteNode> get_links()
    {
        List<NoteNode> links = new List<NoteNode>();

        string[] lines = this.real_file_contents.Split("\n");

        foreach (string line in lines)
        {
            string line_str = line;
            // System.Console.WriteLine(line);
            var line_has_link = line_str.Contains("[[");
            var line_has_embedded_link = line_str.Contains("![[");
            while (line_has_link || line_has_embedded_link)
            {
                if (!line.Contains("]]")) break;

                var start = line_str.IndexOf("[[") + 2;
                var end = line_str.IndexOf("]]");

                if (end - start < 0) break;

                var keyword = line_str.Substring(start, end - start);

                if (blacklist != null)
                {
                    if (blacklist.Contains(keyword)) break;
                    if (!set.Contains(keyword))
                    {

                        links.Add(new NoteNode(keyword, set, this, log_file, blacklist));
                    }

                }

                line_str = line_str.Substring(start, line_str.Length - start);

                line_has_link = line_str.Contains("[[");
                line_has_embedded_link = line_str.Contains("![[");

            }
        }
        return links;
        // throw new NotImplementedException();
    }


    public static string get_filepath_from_name(string linkname, string working_directory_str = ".")
    {
        var files = Directory.GetFiles(working_directory_str);
        foreach (var file in files)
        {
            if (file.Contains(linkname + ".md")) return file;
        }
        var directories = Directory.GetDirectories(working_directory_str);
        foreach (string directory in directories)
        {
            var file = get_filepath_from_name(linkname, directory);
            if (file != null) return file;
        }
        return "";
    }

    public int note_card_tree_size(int answer = 0)
    {

        answer += links.Count;
        foreach (var link in links)
        {
            answer = link.note_card_tree_size(answer);
        }


        return answer;
        // throw new NotImplementedException();
    }

    public int content_size(int answer = 0)
    {

        answer += Marshal.SizeOf(this);
        foreach (var link in links)
        {
            answer += link.content_size(answer);
        }
        // throw new NotImplementedException();
        return answer;
    }

    public void generate_cards()
    {
        if (this.real_file_contents == null)
        {
            this.note_card_contents = "";
            return;
        }


        String reference_buffer = this.real_file_contents.ToString();
        var new_buffer = this.real_file_contents.ToString();

        // string full_definition = "";

        var has_embedded_link = reference_buffer.Contains("![[");
        while (has_embedded_link)
        {
            var start = reference_buffer.IndexOf("![[");
            var end = reference_buffer.IndexOf("]]");
            if (end < start) break;
            var keyword = reference_buffer.Substring(start, end - start);
            NoteNode embedded_note = null;
            foreach (var link in links)
            {
                if (link.name == keyword)
                {
                    embedded_note = link;
                    break;
                }
            }
            if (embedded_note == null) break;
            if (embedded_note.note_card_contents == null) embedded_note.generate_cards();
            new_buffer = new_buffer.Replace("![[" + keyword + "]]", embedded_note.note_card_contents);
            reference_buffer = reference_buffer.Substring(end + 2);
            has_embedded_link = reference_buffer.Contains("![[");
        }
        if (new_buffer.Contains("[["))
        {
            new_buffer = new_buffer.Replace("[[", " ");
        }
        if (new_buffer.Contains("]]"))
        {
            new_buffer = new_buffer.Replace("]]", " ");
        }
        string[] keywords = this.name.Split(" ");


        foreach (var keyword in keywords)
        {
            if (keyword == "") continue;
            if (keyword.Length == 1) continue;
            string blank_with_correct_size = "";
            for (int i = 0; i < keyword.Length; i++)
            {
                blank_with_correct_size += "_";
            }
            blank_with_correct_size = " " + blank_with_correct_size + " ";
            if (new_buffer.Contains(" " + keyword + " "))
            {
                new_buffer = new_buffer.Replace(keyword, blank_with_correct_size);
            }
        }
        this.note_card_contents = "";
        this.note_card_contents += new_buffer;
        foreach (var link in links)
        {
            link.generate_cards();
        }
        // throw new NotImplementedException();
    }

    public void write_cards(StreamWriter output_file)
    {
        foreach (var link in this.links)
        {
            link.write_cards(output_file);
        }
        string final_formatted_card = name.ToString();
        final_formatted_card += ":";
        // if (this.note_card_contents == "") System.Console.WriteLine("{0} is apparently empty", this.name);
        final_formatted_card += this.note_card_contents;
        final_formatted_card += ";";
        // final_formatted_card.Append(note_card_contents);
        // final_formatted_card.Append(";");
        output_file.WriteLine(final_formatted_card);
        // throw new NotImplementedException();
    }

    public List<string> find_all_blank_notes(List<string> answer = null)
    {
        if (answer == null)
        {
            answer = new List<string>();
        }
        foreach (var link in this.links)
        {
            if (link.real_file_contents == "" || link.real_file_contents == null)
            {
                answer.Add(link.name);
            }
            else
            {
                link.find_all_blank_notes(answer);
            }
        }


        return answer;
    }

    public void generate_blank_notes_list_file()
    {
        List<string> blank_notes = find_all_blank_notes();
        System.Console.WriteLine("number of blank notes: " + blank_notes.Count);
        StreamWriter streamWriter = new StreamWriter(this.name + " Blank Notes.md");
        foreach (var note in blank_notes)
        {
            string line = " - [[" + note + "]]";
            streamWriter.WriteLine(line);
        }
        streamWriter.Close();
        // throw new NotImplementedException();
    }
}