# SignalR и Blazor
В этом задании предстоит внедрить использование SignalR и Blazor
в существующий проект.

# Подготовка
Перед началом выполнения задания полезно вспомнить, как в JavaScript происходит обращение к элементам страницы. Также полезно почитать, что такое long polling и web sockets.

# 1. Переход к JavaScript
Перед нами уже знакомый нам проект - BadNews. Владельцы сайта захотели увеличить его посещаемость. В качестве одного из путей достижения своей цели они выбрали добавление комментариев - так они надеются, что люди захотят подольше оставаться на портале. При этом они слышали, что везде происходит переход на JavaScript для рендеринга страниц. Давайте мы постепенно начнём его внедрять.

## 1.1. Подготовка API
Для начала нам нужно подготовить API, откуда будут получаться текущие комментарии для новости. Репозиторий уже готов и лежит в классе `CommentsRepository.cs`. Нужно его зарегистрировать в контейнере.

Создайте новый класс `CommentDto` по пути `Models\Comments`. Поместите туда следующий код:
```cs
namespace BadNews.Models.Comments
{
    public class CommentDto
    {
        public string User { get; set; }
        
        public string Value { get; set; }
    }
}
```
Теперь сделайте общую модель для коллекции комментариев `CommentsDto`:
```cs
using System;
using System.Collections.Generic;
using BadNews.Repositories.Comments;

namespace BadNews.Models.Comments
{
    public class CommentsDto
    {
        public Guid NewsId { get; set; }

        public IReadOnlyCollection<CommentDto> Comments { get; set; }
    }
}
```

Для этого создайте новый класс `CommentsController` из следующего шаблона:
```cs
using System;
using System.Linq;
using BadNews.Models.Comments;
using BadNews.Repositories.Comments;
using Microsoft.AspNetCore.Mvc;

namespace BadNews.Controllers
{
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentsRepository commentsRepository;

        public CommentsController(CommentsRepository commentsRepository)
        {
            this.commentsRepository = commentsRepository;
        }

        // GET
        [HttpGet("api/news/{id}/comments")]
        public ActionResult<CommentsDto> GetCommentsForNews(Guid newsId)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
```
Метод реализуйте самостоятельно.

## 1.2. JavaScript
Для начала подключим JQuery, чтобы удобнее было делать запросы к API. В проекте она уже есть, осталось подключить на страницу. Откройте `Views\News\FullArticle.html` и добавьте после импортов строчку
```html
<script src="/lib/jquery/dist/jquery.min.js"></script>
```

Затем добавьте блок для комментариев после текста новости:
```html
<div id="comments" />
```

Теперь сделаем запрос к нашему API и вставим полученные комментарии в код страницы:
```js
<script type="text/javascript">
    $.get(`/api/news/@Model.Article.Id.ToString()/comments`, function(data) {
        const commentsDiv = document.getElementById('comments');
        
        for (const comment of data.comments) {
            const li = document.createElement("li");
            li.textContent = `${comment.user} говорит: ${comment.value}`;
            commentsDiv.appendChild(li);
        }
    });
</script>
```

Теперь попробуйте открыть любую новость и проверьте, что комментарии вставились.

# 2. SignalR
Всё хорошо, комментарии загружаются, но босс хочет их обновление в реальном времени. Настала пора подключить SignalR!

## 2.1. Серверная часть
Нужно создать так называемый "хаб". Хаб - это класс-посредник между серверной и клиентской частью. Он определяет, куда и в каком виде будет отправлено сообщение от сервера.

Создайте папку Hubs, а в ней класс `CommentsHub.cs` с содержимым:
```cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BadNews.Hubs
{
    public class CommentsHub: Hub
    {
        public async Task SendComment(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveComment", user, message).ConfigureAwait(false);
        }
    }
}
```
Теперь в `Startup.cs` добавьте строку
```cs
services.AddSignalR();
```
в `ConfigureServices`, а в `Configure` - эту строку:
```cs
endpoints.MapHub<CommentsHub>("/commentsHub");
```

## 2.2. Отправка сообщений
Подготовимся к тестированию и создадим поля для отправки сообщений. В `FullArtice.cshtml` добавьте следующие элементы:
```html
<label>
    Пользователь <input id="userInput">
</label>
<label>
    Комментарий <input id="commentInput">
</label>
<div>
    <button id="sendButton">Отправить</button>
</div>
```

## 2.3. Клиентская часть.

Для начала установим менеджер пакетов `libman` командой
```
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```
Если вы используете Linux с zsh или MacOS, вы можете встретиться с похожей ошибкой:
```
~ % dotnet tool install --global Microsoft.Web.LibraryManager.CLI
Tools directory '/Users/jimmy/.dotnet/tools' is not currently on the PATH environment variable.
If you are using zsh, you can add it to your profile by running the following command:
cat << \EOF >> ~/.zprofile
# Add .NET Core SDK tools
export PATH="$PATH:/Users/jimmy/.dotnet/tools"
EOF
And run zsh -l to make it available for current session.
You can only add it to the current session by running the following command:
export PATH="$PATH:/Users/jimmy/.dotnet/tools"
You can invoke the tool using the following command: libman
Tool 'microsoft.web.librarymanager.cli' (version '2.0.96') was successfully installed.
```
В сообщении уже написаны дальнейшие шаги. Нужны выполнить команды
```
cat << \EOF >> ~/.zprofile
# Add .NET Core SDK tools
export PATH="$PATH:/Users/jimmy/.dotnet/tools"
EOF
```
и `zsh -l`

После установки libman добавим клиентскую библиотеку (вызывайте из проекта BadNews):
```
libman install @microsoft/signalr@latest -p unpkg -d wwwroot/js/signalr --files dist/browser/signalr.js --files dist/browser/signalr.min.js
```

Подключим в FullArticle.cshtml скрипт SignalR:
```cs
<script src="/js/signalr/dist/browser/signalr.js"></script>
```

Замените наш скрипт на следующий:
```js
$.get(`/api/news/@Model.Article.Id.ToString()/comments`, function(data) {
    const commentsDiv = document.getElementById('comments');
    
    for (const comment of data.comments) {
        const li = document.createElement("li");
        li.textContent = `${comment.user} говорит: ${comment.value}`;
        commentsDiv.appendChild(li);
    }
});

const connection = new signalR.HubConnectionBuilder().withUrl("/commentsHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveComment", function (user, message) {
    const li = document.createElement("li");
    document.getElementById("comments").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} говорит: ${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    const user = document.getElementById("userInput").value;
    const message = document.getElementById("commentInput").value;
    connection.invoke("SendComment", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

```
Теперь попробуйте отправить любой комментарий. Он должен появиться у вас на странице. Проверьте так же, что всё работает, если вы отправляете комментарий из другой вкладки.

Чтобы посмотреть, что за сообщения передаются между страницей и бэкендом, зайдите в инструменты разработчика, перейдите на вкладку Network (Сеть). На вкладке All можно увидеть запрос на подключение к хабу, а на вкладке WS - соединение по WebSocket'ам и сами сообщения.

# 3. Blazor
Босс обрадовался тому, что вы сделали, и решил усилить вашу команду, дав вам помощника. К сожалению, с JavaScript он совсем не дружит, а задачу ему уже дали... Настало время снова обратиться к C#, а точнее, - к Blazor.

# 3.1. Компонент Blazor.
Давайте теперь выделим компонент с комментариями, переписав его сразу на Blazor. При этом будем руководствоваться следующим подходом: изменения можно внедрять постепенно.

Создайте папку BlazorComponents, а в ней - файл `Comments.razor` со следующим содержимым:
```cs
@using BadNews.Repositories.Comments
@inject CommentsRepository commentsRepository;
<div>
    @foreach (var comment in comments)
    {
        <li>@RenderComment(comment)</li>
    }
</div>

@code {
    private IReadOnlyCollection<Comment> comments = new List<Comment>();

    [Parameter]
    public Guid ArticleId { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        comments = commentsRepository.GetComments(ArticleId);
        return base.SetParametersAsync(parameters);
    }

    private string RenderComment(Comment comment) => $"{comment.User} говорит: {comment.Value}";
}
```
Разберём код. Бывает два вида компонентов: компонент и страница. У страницы таке указывается путь, по которому она доступн. Код компонента стоит из двух частей: разметка (как в Razor) и код на C#, где описаны поля, параметры и код.

Директива `@using` служит для различных импортов, `@inject` - для внедрения зависимостей (из контейнера). Атрибут `[Parameter]` служит для обозначения внешних параметров компонента.

Метод `SetParameteresAsync` - один из методов жизненного цикла компонента, служит для заполнения свойств, использующихся в разметке.

Теперь вставим компонент в `FullArticle.cshtml` строчкой
```cs
@(await Html.RenderComponentAsync<Comments>(RenderMode.ServerPrerendered))
```
Существует несколько режимов рендеринга: статический, серверный (два вида) и клиентский (тоже два вида). В данном случае мы используем серверный рендеринг, потому что он проще подключается. Отличие Prerendered от обычного режима в том, что в обычном режиме компонент не вставляется в html-код страницы, поэтому он может не отображаться.

Уберите ещё все js-скрипты и их импорты - они нам не понадобятся.

Добавьте в `Startup.cs` в `Configure` строчку
```cs
endpoints.MapBlazorHub();
```
а в `ConfigureServices` - строчку
```cs
services.AddServerSideBlazor();
```

Также очистите опцию `UseStaticFiles` (оставьте только вызов `app.UseStaticFiles()`);

Создайте в корне проекта файл `_Imports.razor` с содержимым
```cs
@using Microsoft.AspNetCore.Authorization;
@using Microsoft.AspNetCore.Components.Authorization;
@using Microsoft.AspNetCore.Components.Forms;
@using Microsoft.AspNetCore.Components.Routing;
@using Microsoft.AspNetCore.Components.Web;
```

В _Layout.cshtml добавьте две строчки:
```cs
<base href="~/" />
...
<script src="_framework/blazor.server.js"></script>
```


Проверьте, что после этого на странице какой-нибудь новости отоюражаются комментарии.

# 3.2. Blazor и SignalR.
Настала пора вернуть интерактив в комментарии. Подключим SignalR.

Установите пакет `Microsoft.AspNetCore.SignalR.Client` версии 5.0.4. Теперь замените компонент `Comments.razor` на следующий код:
```cs
@using BadNews.Repositories.Comments
@using Microsoft.AspNetCore.SignalR.Client
@inject CommentsRepository commentsRepository;
@inject NavigationManager navigationManager
@implements IAsyncDisposable
<div>
    <label>
        Пользователь: <input @bind="userInput"/>
    </label>
    <label>
        Сообщение: <input @bind="messageInput"/>
    </label>
    <div>
        <button @onclick="Send">Отправить</button>
    </div>
    @foreach (var comment in comments)
    {
        <li>@RenderComment(comment)</li>
    }
</div>

@code {
    private HubConnection hubConnection;
    private List<Comment> comments = new();

    private string userInput;
    private string messageInput;

    [Parameter]
    public Guid ArticleId { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        comments = commentsRepository.GetComments(ArticleId).ToList();
        return base.SetParametersAsync(parameters);
    }
    
    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/commentsHub"))
            .Build();   
        
        hubConnection.On<string, string>("ReceiveComment", (user, message) =>
        {
            var comment = new Comment(user, message);
            comments.Add(comment);
            StateHasChanged();
        });

        await hubConnection.StartAsync().ConfigureAwait(false);
    }

    private async Task Send()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendComment", userInput, messageInput);
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

    private string RenderComment(Comment comment) => $"{comment.User} говорит: {comment.Value}";
}
```
а в `FullArticle.cshtml` уберите инпуты с кнопкой.

Теперь проверьте, что у вас отправляются комментарии.