using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Model;

namespace AudioStation.Component
{
    public class LibraryPlayer
    {
        private List<LibraryEntry> _playlist;

        public LibraryPlayer()
        {
            _playlist = new List<LibraryEntry>();

            NAudio.Mixer.Mixer s;
        }


    }
}
