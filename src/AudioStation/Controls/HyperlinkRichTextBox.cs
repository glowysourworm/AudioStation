using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Controls
{
    public class HyperlinkRichTextBox : RichTextBox
    {
        public static readonly DependencyProperty TextProperty 
            = DependencyProperty.Register("Text", typeof(string), typeof(HyperlinkRichTextBox), new PropertyMetadata(string.Empty, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private const string URL_REGEX = "([\\w+]+\\:\\/\\/)?([\\w\\d-]+\\.)*[\\w-]+[\\.\\:]\\w+([\\/\\?\\=\\&\\#.]?[\\w-]+)*\\/?";

        private readonly IOutputController _outputController;

        public HyperlinkRichTextBox()
        {
            _outputController = IocContainer.Get<IOutputController>();
        }

        private void Reload()
        {
            var document = new FlowDocument();
            var regex = new Regex(URL_REGEX);

            foreach (var paragraph in this.Text.Split('\n'))
            {
                var documentParagraph = new Paragraph();
                //var linkMatches = regex.Matches(paragraph);

                var lastWord = string.Empty;

                for (int index = 0; index < paragraph.Length; index++)
                {
                    //var match = linkMatches.FirstOrDefault(x => x.Index == index);
                    /*
                    // Link Start
                    if (match != null)
                    {
                        var linkURL = this.Text.Substring(match.Index, match.Length);

                        Hyperlink link = new Hyperlink();
                        link.IsEnabled = true;
                        link.Inlines.Add(linkURL);
                        link.NavigateUri = new Uri(linkURL);
                        link.RequestNavigate += RequestNavigate;
                        documentParagraph.Inlines.Add(link);

                        // Reset the "word"
                        lastWord = string.Empty;

                        // Jump ahead to the end of the link
                        index = match.Index + match.Length;
                    }

                    // Other Character
                    else
                    {*/
                        if (paragraph[index] == ' ')
                        {
                            documentParagraph.Inlines.Add(lastWord);
                            documentParagraph.Inlines.Add(" ");
                            lastWord = string.Empty;
                        }
                        else
                            lastWord += paragraph[index];
                    //}
                }

                documentParagraph.Inlines.Add(lastWord);

                document.Blocks.Add(documentParagraph);
            }


            // Finally, set the document
            this.Document = document;
        }

        private void RequestNavigate(object sender, RequestNavigateEventArgs args)
        {
            try
            {
                Process.Start(args.Uri.ToString());
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error navigating to URL:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
            }
        }

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (obj as HyperlinkRichTextBox);
            if (control != null)
            {
                control.Reload();
            }
        }
    }
}
