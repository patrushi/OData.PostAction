OData.PostAction

Add post action to odata controller action.

Do not working with expand collection.

# Using 

```
...
public IActionResult GetMyEntity(ODataQueryOptions<MyEntity> options)
{
    var query = DbContext.MyEntity;

    Action<ICollection<MyEntity>> postAction = collection => {
        var keys = collection.Select(e => e.RemoteEntityId).Distinct().ToArray();
        var remoteEntityList = RemoteApi.GetRemoteEntityList(keys).ToDictionary(e => e.Key);
        collection.ForEach(e => e.RemoteEntity = e.RemoteEntityId == null ? null : remoteEntityList.GetValueOrDefault(e.RemoteEntityId));
    };
    
    return PostAction.PostActionOnQuery(query, options, postAction);
}
...
```

# Links
* Nuget - https://www.nuget.org/packages/OData.PostAction/

# Thanks
* https://github.com/gorillapower for https://github.com/OData/WebApi/issues/521#issuecomment-281309496
* https://stackoverflow.com/users/3191951/kedrzu for https://stackoverflow.com/questions/47094145/how-to-fill-some-properties-in-model-after-evaluation-of-iqueryable-in-odata/47113000#47113000