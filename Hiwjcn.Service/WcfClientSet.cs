using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc;
using Lib.rpc;
using QPL.WebService.Product.Core;
using System.Configuration;

namespace Hiwjcn.Bll
{
    public class ProductServiceClient : ServiceClient<IProduct>
    {
        public ProductServiceClient() : base("http://service.qipeilong.net:9006/wsProduct/Product.svc")
        {
            this.Endpoint.EndpointBehaviors.Add(new MyEndPointBehavior());
        }
    }
}
