//    NooseMod LCPDFR Plugin with Database System
//    StatsForm: Form of Stats Recording
//    Copyright (C) 2017 Naruto 607

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.

//    Greetings to Sam @ LCPDFR.com for this wonderful API feature.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// The use of this StatsForm is beta - it is used to be implemented into the LCPDFR Police Computer
// so that the Ms. Access Database file can be accessed live (regardless of time and place).

namespace NooseMod_LCPDFR
{
    public partial class StatsForm : Form
    {
        public StatsForm()
        {
            InitializeComponent();
        }

        private void StatsForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'statsDataSet.OverallStats' table. You can move, or remove it, as needed.
            this.overallStatsTableAdapter.Fill(this.statsDataSet.OverallStats);
            // TODO: This line of code loads data into the 'statsDataSet.MissionStats' table. You can move, or remove it, as needed.
            this.missionStatsTableAdapter.Fill(this.statsDataSet.MissionStats);

        }
    }
}
