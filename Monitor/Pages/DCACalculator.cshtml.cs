using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using Core.Main;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;

namespace Monitor.Pages {
  public class DCACalculatorModel : _Internal.BasePageModelSecure {
    public ProfitTrailerData PTData = null;

    public void OnGet() {
      base.Init();

      BindData();
    }

    private void BindData() {
      PTData = new ProfitTrailerData(PTMagicBasePath, PTMagicConfiguration);
    }
  }
}
