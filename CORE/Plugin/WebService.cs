using Microsoft.AspNetCore.Mvc;

namespace core.webservice{
    public interface Iwebservice{
        IActionResult GetById(string id);
    }

}