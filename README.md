# Note Card Generator (C#)

Rewrite of a C automation script I made in May to generate a txt file organizing the note constelation of a particular obsidian note/topic into a format that can be imported into Quizlet for studying.

Usage:
```
./note_card_generator_csharp {target_subject}
./note_card_generator_csharp {target_subject} {blacklisted_notes}
```

Specifically, this script reads the target file specified by the user, and recursively reads every file linked in the target, and continues untill there are no subtopics to read.

In the case there is a note/topic that the user doesn't want to include in the note card set, the user specifies it in the arguments.
<!-- 
The output of the automation script is as follows: 
 - {target_subject}_quizlet.txt - the text file that contains the notecard set
 - {target_subject}_chain.txt - text file that shows the chain of notes that  -->