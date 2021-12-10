using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace V3UPSManager
{
    internal class DisplayInfo
    {

        public DisplayInfo()
        {

        }

        internal static DialogResult Prompt(MessageBoxButtons btn, params string[] lines)
        {
            SystemSounds.Question.Play();
            string msg = String.Join(Environment.NewLine + Environment.NewLine, lines);
            return System.Windows.Forms.MessageBox.Show(msg, "Prompt", btn, MessageBoxIcon.Asterisk);
        }

        internal static DialogResult Print(string line)
        {
            MessageBoxButtons btn = new MessageBoxButtons();
            return System.Windows.Forms.MessageBox.Show(line, "Prompt", btn, MessageBoxIcon.Asterisk);
        }

        internal static DialogResult Ask(string line)
        {
            return System.Windows.Forms.MessageBox.Show(line, "Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
        }
    }
}
