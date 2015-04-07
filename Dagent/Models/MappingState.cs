using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dagent.Library;
using Dagent;
using Dagent.Rows;

namespace Dagent.Models
{
   internal class MappingState<T> : IMappingState<T>
    {
        public MappingState()
        {        
            NewModel = true;                        
        }
        
        public object Tag { get; set; }                
        public int RowIndex { get; set; }        
        public bool NewModel { get; set; }
        public bool Break { get; set; }
        public Func<INextRow, bool> RequestNewModel { get; set; }
    }
}

    
