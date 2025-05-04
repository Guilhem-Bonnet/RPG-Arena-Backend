using System.Text;

namespace RPGArena.CombatEngine.Logging
{
    /// <summary>
    /// Représente un message composé de segments textuels, chacun pouvant avoir une couleur différente.
    /// Utilisé pour logger les événements du combat avec mise en forme.
    /// </summary>
    public class Message
    {
        public List<Segment> Segments { get; } = new();

        /// <summary>
        /// Ajoute un segment de texte avec une couleur facultative.
        /// </summary>
        public Message AddSegment(string content, ConsoleColor? color = null)
        {
            Segments.Add(new Segment(content, color));
            return this;
        }

        /// <summary>
        /// Ajoute plusieurs segments provenant d’un autre message.
        /// </summary>
        public void AddSegmentsFrom(Message other)
        {
            if (other == null) return;
            Segments.AddRange(other.Segments);
        }

        /// <summary>
        /// Convertit tous les segments en une chaîne brute, sans couleurs.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var segment in Segments)
            {
                sb.Append(segment?.Content);
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Un segment individuel de texte, éventuellement coloré pour affichage console.
    /// </summary>
    public class Segment
    {
        public string Content { get; }
        public ConsoleColor? Color { get; }

        public Segment(string content, ConsoleColor? color = null)
        {
            Content = content ?? string.Empty;
            Color = color;
        }
    }
}