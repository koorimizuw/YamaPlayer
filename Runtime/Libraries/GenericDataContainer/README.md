# GenericDataContainer

A generic and type-safe wrapper for DataContainer.

## Installation

To use this package, you need to add [my package repository](https://github.com/koyashiro/vpm-repos).
Please read more details [here](https://github.com/koyashiro/vpm-repos#installation).

Please install this package with [Creator Companion](https://vcc.docs.vrchat.com/) or [VPM CLI](https://vcc.docs.vrchat.com/vpm/cli/).

### Creator Companion

1. Enable the `koyashiro` package repository.

   ![image](https://user-images.githubusercontent.com/6698252/230629434-048cde39-a0ec-4c53-bfe2-46bde2e6a57a.png)

2. Find `GenericDataContainer` from the list of packages and install any version you want.

### VPM CLI

1. Execute the following command to install the package.

   ```sh
   vpm add package net.koyashiro.genericdatacontainer
   ```

## `DataList<T>`

### Example

```cs
using UnityEngine;
using UdonSharp;
using Koyashiro.GenericDataContainer;

public class DataListExample : UdonSharpBehaviour
{
    public void Start()
    {
        DataList<int> list = DataList<int>.New();

        list.Add(100);
        list.Add(200);
        list.Add(300);

        Debug.Log(list.GetValue(0)); // 100
        Debug.Log(list.GetValue(1)); // 200
        Debug.Log(list.GetValue(2)); // 300

        int[] array = list.ToArray();
    }
}
```

## `DataDictionary<TKey, TValue>`

### Example

```cs
using UnityEngine;
using UdonSharp;
using Koyashiro.GenericDataContainer;

public class DataDictionaryExample : UdonSharpBehaviour
{
    public void Start()
    {
        DataDictionary<string, int> dic = DataDictionary<string, int>.New();

        dic.SetValue("first", 100);
        dic.SetValue("second", 200);
        dic.SetValue("third", 300);

        Debug.Log(dic.GetValue("first")); // 100
        Debug.Log(dic.GetValue("second")); // 200
        Debug.Log(dic.GetValue("third")); // 300
    }
}
```
