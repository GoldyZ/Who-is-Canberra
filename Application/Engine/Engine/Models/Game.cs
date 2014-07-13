using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Engine.Models
{
    public class Game
    {
        private static Dictionary<Guid, Game> _active = new Dictionary<Guid, Game>();

        public static Game GetOrCreateGame(Guid id) 
        {
            Game game;

            if (_active.TryGetValue(id, out game))
                return game;

            game = new Game(id);
            _active[id] = game;
            return game;

        }

        public Game(Guid id)
        {
            this.ID = id;
        }

        public Guid ID { get; set; }

        public Result Current { get; set; }

        public DateTime Expiry { get; set; }


        public Result GetNextQuestion()
        {
            if (this.Current == null || DateTime.Now > this.Expiry)
            {
                using (var repository = new Repository())
                {
                    repository.BeginTransaction();

                    var datapoint = repository.GetNextDatapoint();
                    if (datapoint == null)
                        return null;

                    repository.SetDatapointUsed(datapoint.ID);
                    var related = repository.GetRelatedDatapoints(datapoint.ID);

                    this.Current = new Result();
                    this.Current.Focus = datapoint;
                    this.Current.Related = related;

                    this.Expiry = DateTime.Now.AddSeconds(Constants.QuestionTimeout);

                    repository.CommitTransaction();
                }
            }

            this.Current.ExpiresIn = this.Expiry.Subtract(DateTime.Now).TotalSeconds;

            return this.Current;
        }
    }
}