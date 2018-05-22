using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Core.Main;
using Core.Main.DataObjects;
using Core.Main.DataObjects.PTMagicData;

namespace Monitor.Pages {
  public class BagAnalyzerModel : _Internal.BasePageModelSecure {
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
