using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenScribe.Helpers
{
    public static class GenericHelper
    {
        public static int CountWords(string text)
        {
            int _wordCount = 0, index = 0;

            // skip whitespace until first word
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;

            while (index < text.Length)
            {
                // check if current char is part of a word
                while (index < text.Length && !char.IsWhiteSpace(text[index]))
                    index++;

                _wordCount++;

                // skip whitespace until next word
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                    index++;
            }
            return _wordCount;
        }

        public static List<string> LoadFonts() {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            path += "\\Data\\fonts.txt";
            string contents = File.ReadAllText(path);
            return contents.Split(Environment.NewLine).ToList();
        }

        public static void LoadColorSchemes(List<Color> backgrounds, List<Color> foregrounds) {
            (string Background, string Foreground)[] schemes =
            {
                ("#000000", "#FFFFFF"),
                ("#222323", "#F0F6F0"),
                ("#382B26", "#B8C2B9"),
                ("#3E232C", "#EDF6D6"),
                ("#172554", "#DBEAFE"),
                ("#17352D", "#D9F2E6"),
                ("#30213D", "#F3E8FF"),
                ("#F2E8D5", "#2B2520")
            };

            foreach ((string background, string foreground) in schemes)
            {
                backgrounds.Add(ColorTranslator.FromHtml(background));
                foregrounds.Add(ColorTranslator.FromHtml(foreground));
            }
        }

        public static List<(string Shortcut, string Description)> LoadCommandList() {
            List<(string Shortcut, string Description)> commandList = new List<(string, string)>
            {
                ("F1", "Mostra questa schermata"),
                ("Esc", "Chiude la schermata di aiuto"),
                ("Ctrl + S", "Salva il file o sceglie dove salvarlo"),
                ("Ctrl + O", "Apre un file"),
                ("Ctrl + F", "Cerca un testo"),
                ("Ctrl + N", "Salva il file aperto e svuota la bozza"),
                ("Ctrl + M", "Cambia traccia o disattiva la musica"),
                ("Alt + =", "Aumenta la dimensione del carattere"),
                ("Alt + -", "Riduce la dimensione del carattere"),
                ("Alt + C", "Scorre gli schemi colore e la trasparenza"),
                ("Alt + F", "Cambia carattere"),
                ("Alt + T", "Attiva o disattiva la trasparenza"),
                ("Alt + W", "Cambia la larghezza del testo"),
                ("Ctrl + Alt + B", "Sposta la barra dati in alto o in basso"),
                ("Invio", "Va a capo o conferma la ricerca"),
                ("Tab", "Inserisce un rientro")
            };
            return commandList;
        }
    }

    
}
