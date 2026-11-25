/*
   Copyright 2016-2022 LIPtoH <liptoh.codebase@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

//---------------------------------------------------------------------------
// Project:           Adobe Color Picker Clone 1
// Filename:          PsColorPicker.cs
// Original Author:   Danny Blanchard
//                    - scrabcakes@gmail.com
// Updates:
//  3/28/2005 - Initial Version : Danny Blanchard
//  July 2010 - Updated by OpenPainter.org
//---------------------------------------------------------------------------

using System.Drawing;
namespace OpenPainter.ColorPicker
{
    public class FormColorPicker : TS_SE_Tool.MaterialColorDialog
    {
        public FormColorPicker(Color startingColor) : base(startingColor) { }
    }
}
