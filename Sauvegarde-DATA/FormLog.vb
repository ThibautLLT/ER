Imports System.IO
Imports System.IO.File


Public Class FormLog
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim LogFolder As String
        LogFolder = My.Application.Info.DirectoryPath + "\logFolder\" 'répertoire de sauvegarde des logs
        For Each s As String In System.IO.Directory.GetFiles(LogFolder) 'détection des fichiers de logs existant
            ComboBox1.Items.Add(s.Replace(LogFolder, "").Replace(".txt", "")) 'ajout des logs dans la combobox
        Next
    End Sub

    ' functions pour écrire un string dans les logs
    Function EcrireLog(TEXT)
        Dim filename As String = Application.StartupPath & "\logFolder\" + Now.ToShortDateString().Replace("/", "_") + ".txt"
        Dim sw As StreamWriter = AppendText(filename)
        sw.WriteLine(Now() & " " & TEXT)
        sw.Close()
    End Function

    ' affichage du log dans le RichTextBox1
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim target As String = My.Application.Info.DirectoryPath + "\logFolder\" + ComboBox1.Text + ".txt"
        RichTextBox1.Text = My.Computer.FileSystem.ReadAllText(target)
    End Sub

End Class
