var shuffle = function(o) {
    for (var j, x, i = o.length; i; j = Math.floor(Math.random() * i), x = o[--i], o[i] = o[j], o[j] = x);
    return o;
};

var getRandomInt = function(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
};

$(function () {
    var $possibles = $('ul.possibles');
    var $dots = $('ul.dots');
    var dotDelay = 2000;
    var blankColor = '#666';
    var colors = [
        { code: '#ffffff', name: 'White' },
        { code: '#2edff2', name: 'Light Blue' },
        { code: '#a54f10', name: 'Brown' },
        { code: '#f9b323', name: 'Yellow' }
    ];

    var reset = function () {
        $('p.answer').empty();
        $('p.question').empty();

        $('p.caption').empty();
        $('p.credit').empty();
        $("#displayImage").hide();

        $possibles.empty();

        $dots.find("li.datapoint").animate({
            backgroundColor: blankColor
        }, dotDelay /2 );

        window.setTimeout(nextQuestion, dotDelay);
    };

    var nextQuestion = function () {
        $.get('/GetNextQuestion?id=f6aba1b5-83cd-49b4-a142-5d3d5e1c9cd1', {}, function (response) {

            var question = response.Focus.Questions[getRandomInt(0, response.Focus.Questions.length - 1)];

            var data = {
                'area': question.Area,
                'country': response.Focus.Country,
                'ratio': response.Focus.Ratio
            };

            $('p.question').text(tmpl(question.Text, data));

            // get image
            var imageURL = response.Focus.Endplates[getRandomInt(0, response.Focus.Endplates.length - 1)].Uri;

            $("#displayImage").hide();
            $('#displayImage').prop('src', imageURL);


            var answers = response.Related.slice(0);
            answers.push(response.Focus);
            shuffle(answers);

            var $dot = $dots.find('li:first');

            for (var j = 0; j < answers.length; j++) {
                var c = colors[j];
                answers[j].Color = c.name;

                for (var k = 0; k < answers[j].Ratio; k++) {
                    $dot.css('backgroundColor', c.code);
                    $dot = $dot.next();
                }
            }

            for (var i = 0; i < answers.length; i++) {
                var text = (question.Type == 0 ? answers[i].Color : answers[i].Country);
                var $answerLabel = $('<li>' + text + '</li>').appendTo($possibles);

                if (answers[i].Questions) {
                    $answerLabel.addClass('answer');
                }
            }

            window.setTimeout(function () {
                showAnswer(question, response);
            }, response.ExpiresIn * 1000);
        });
    };

    var showAnswer = function (question, response) {
        var text = (question.Type == 0 ? response.Focus.Color : response.Focus.Country);
        $('p.answer').text(text);

        var imageCaption = response.Focus.Endplates[0].Caption;
        var imageURL = response.Focus.Endplates[0].Uri;
        var imageCredit = response.Focus.Endplates[0].Credit;

        $('p.caption').text(imageCaption);
        $('p.credit').text(imageCredit);
        $('#displayImage').prop('src', imageURL);
          $('#displayImage').show();

        window.setTimeout(reset, 10000);
    };

    for (var i = 0; i < 100; i++) {
        $('<li class="datapoint" style="display: hidden;"></li>')
            .appendTo($dots);
    }

    reset();
});