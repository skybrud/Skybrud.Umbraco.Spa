using Skybrud.Umbraco.GridData;
using Skybrud.Umbraco.GridData.Interfaces;

namespace Skybrud.Umbraco.Spa.Models.Grid {

    public class SpaGridControlValueBase : IGridControlValue {

		public GridControl Control { get; }

		public virtual bool IsValid => true;

        public virtual string GetSearchableText() {
			return string.Empty;
		}

        public virtual SpaGridControl GetControlForSpa() {
            return new SpaGridControl(this, Control.Editor.Alias);
        }

        public SpaGridControlValueBase(GridControl control) {
			Control = control;
		}

	}

}