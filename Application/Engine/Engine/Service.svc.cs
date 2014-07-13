using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Engine
{
    public class Service : IService
    {
        public Models.Result GetNextQuestion(Guid id)
        {
            var game = Models.Game.GetOrCreateGame(id);
            return game.GetNextQuestion();
        }
    }
}
