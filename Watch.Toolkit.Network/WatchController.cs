using System.Collections.Generic;
using System.Web.Http;

namespace Watch.Toolkit.Network
{
    public class WatchController : ApiController
    {
        readonly object _dataHolder;

        //public ActivitiesController(object dataholder)
        //{
        //    _dataHolder = dataholder;
        //}

        //public List<IActivity> Get()
        //{
        //    return _system.GetActivities();
        //}

        //public IActivity Get(string id)
        //{
        //    return _system.GetActivity(id);
        //}

        //public void Post(JObject activity)
        //{
        //    _system.AddActivity(Helpers.Json.ConvertFromTypedJson<IActivity>(activity.ToString()));
        //}

        //public void Delete(string id)
        //{
        //    _system.RemoveActivity(id);
        //}

        //public void Put(JObject activity)
        //{
        //    _system.UpdateActivity(Helpers.Json.ConvertFromTypedJson<IActivity>(activity.ToString()));
        //}
    }
}
