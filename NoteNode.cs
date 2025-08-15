using System.Runtime.InteropServices;

namespace jfObsidian;

public class NoteNode
{
    public string name;
    public string filepath;
    public string real_file_contents;
    public string note_card_contents;
    public List<NoteNode> links;
    public NoteNode parent;


    private HashSet<string> set;
    private string v;
    private StreamWriter log_file;
    private List<string> blacklist;

    public NoteNode(string linkname, HashSet<string> set, NoteNode parent = null, StreamWriter notesChainLogFile = null, List<string> blacklist = null)
    {
        this.name = linkname;
        this.filepath = getFilepathFromName(linkname, ".");
        // var stream = File.OpenRead(filepath);
        this.parent = parent;
        this.log_file = notesChainLogFile;
        this.blacklist = blacklist;
        PrintParentLinkChain();
        // if (this.filepath == "") System.Console.WriteLine("cannot find {0}", linkname);
        if (this.filepath == "" || !File.Exists(filepath))
        {
            this.real_file_contents = "";
            // System.Console.WriteLine($"filepath for {name} not found");
        }
        else
        {
            try
            {
                this.real_file_contents = File.ReadAllText(filepath);
            }
            catch (FileNotFoundException)
            {
                System.Console.WriteLine($"file read error {filepath}");
                this.real_file_contents = "";
                // throw;
            }
        }

        this.note_card_contents = "";
        this.set = set;
        set.Add(this.name);
        this.links = getLinks();

    }

    private void PrintParentLinkChain()
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



    public List<NoteNode> getLinks()
    {
        List<NoteNode> links = new List<NoteNode>();

        string[] lines = this.real_file_contents.Split("\n");

        foreach (string line in lines)
        {
            get_links_in_line(links, line);
        }
        return links;
        // throw new NotImplementedException();
    }

    private void get_links_in_line(List<NoteNode> links, string line)
    {
        string line_str = line;
        // System.Console.WriteLine(line);
        var line_has_link = line_str.Contains("[[");
        var line_has_embedded_link = line_str.Contains("![[");
        while (line_has_link || line_has_embedded_link)
        {
            // System.Console.WriteLine(line);
            if (!line.Contains("]]")) break;

            var start = line_str.IndexOf("[[") + 2;
            var end = line_str.IndexOf("]]");

            if (end - start < 0) break;

            var keyword = line_str.Substring(start, end - start);

            if (blacklist != null)
            {
                if (blacklist.Contains(keyword))
                {
                    line_str = line_str.Substring(start, line_str.Length - start);

                    line_has_link = line_str.Contains("[[");
                    line_has_embedded_link = line_str.Contains("![[");

                    continue;
                }
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

    public static string getFilepathFromName(string linkname, string workingDirectoryStr = ".")
    {
        var files = Directory.GetFiles(workingDirectoryStr);

        // System.Console.WriteLine($"files in working directory {}");
        foreach (var file in files)
        {
            // System.Console.WriteLine(file);
            if (linkname.Contains("Every Data Structure") && file.Contains(linkname))
            {
                System.Console.WriteLine($"has keyword {file}");
                // return file;
            }
            if (Path.GetFileName(file) == linkname + ".md")
            {
                return file;
            }
        }
        var directories = Directory.GetDirectories(workingDirectoryStr);
        foreach (string directory in directories)
        {
            // System.Console.WriteLine($"\nlinkname {linkname}\ndirectory {directory}\n");
            var file = getFilepathFromName(linkname, directory);
            if (!string.IsNullOrEmpty(file)) return file;

        }

        return "";
    }

    public int noteCardTreeSize(int answer = 0)
    {

        answer += links.Count;
        foreach (var link in links)
        {
            answer = link.noteCardTreeSize(answer);
        }


        return answer;
        // throw new NotImplementedException();
    }

    public int contentSize(int answer = 0)
    {

        answer += Marshal.SizeOf(this);
        foreach (var link in links)
        {
            answer += link.contentSize(answer);
        }
        // throw new NotImplementedException();
        return answer;
    }

    public void generateCards()
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
            if (embedded_note.note_card_contents == null) embedded_note.generateCards();
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
            link.generateCards();
        }
        // throw new NotImplementedException();
    }

    public void writeCards(StreamWriter outputFile)
    {
        foreach (var link in this.links)
        {
            link.writeCards(outputFile);
        }
        string final_formatted_card = name.ToString();
        final_formatted_card += "::";
        // if (this.note_card_contents == "") System.Console.WriteLine("{0} is apparently empty", this.name);
        final_formatted_card += this.note_card_contents;
        final_formatted_card += ";;";
        // final_formatted_card.Append(note_card_contents);
        // final_formatted_card.Append(";");
        outputFile.WriteLine(final_formatted_card);
        // throw new NotImplementedException();
    }

    public List<string> findAllBlankNotes(List<string> answer = null)
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
                link.findAllBlankNotes(answer);
            }
        }


        return answer;
    }

    public void GenerateBlankNotesListFile()
    {
        List<string> blank_notes = findAllBlankNotes();
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