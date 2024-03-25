/* Заметки касательно Main и InjectedLogic проектов
 * Библиотека InjectDotnet 0.4.0 создана для net6.0-windows7.0 - т.е. для Windows OS начиная с Windows 7, поэтому в проекте использован соответствующий .NET 6 только для Windows
 * Библиотека InjectDotnet 0.4.0 также требует быть использованной либо в x86 либо в x64 билде, не AnyCpu, т.к. в Nuget папке пакетов, InjectDotnet не имеет папки AnyCpu со своей .dll, а лишь папки x86 и x64, а компилятор и Rider в AnyCpu режиме будут искать .dll именно в AnyCpu папке
 * Функция Bootstrap (из .dll которая будет заинжекчена) обязана иметь параметры "IntPtr argument, int size", иначе вызов функции: 0xFF, 0x55, 0x48, //call [Arg._method_fn] (см. CoreLoader класс в InjectDotnet) - упадет без явной ошибки вернув 0xC0000005 (STATUS_ACCESS_VIOLATION)
 * Проект Main не должен ссылаться на проект InjectedLogic, т.к. видимо таком случае вся метаинформация по зависимостям в кот. нуждается InjectedLogic будет хранится в Main проекте, соотв. после инъекции InjectedLogic.dll не сможет подгрузить свои зависимости
   Поэтому InjectedLogic проект должен быть создан как отдельная .dll от которой Main (или любой другой проект injector не зависил, если б был) не зависит
 * Main компилируется в специальную папку bin_solution, перед компиляцией Rider запускает компиляцию InjectedLogic, InjectedLogic при этом компилируется в свою подпапку папки bin_solution
   Дабы положить в конце всего билда InjectedLogic.dll в папку к скомпилированному Main, в него в InjectedLogic.csproj есть post-build event кот. с пом. программы robocopy рекурсивно копирует все .dll из выходной папки скомпилированного InjectedLogic в папку к скомпил. Main
   При этом robocopy обычно может вернуть целых 9 кодов (от 0 до 8) результата работы, где лишь 9-й обозначает ошибку, однако Rider (и Visual Studio) воспиринмают любые числа кроме 0 как ошибку, поэтому приходится вручную все возвращенные коды кроме 8 превращать в 0
   P.S. Rider по умолчанию не вызывает post-build event для проекта не требующего повторный билд, в моем случае это обязательно, поэтому была изменена настройка "Toolset And Build" -> "Invoke pre- and post- build event targets for skipped projects" 
   P.S. Если "post-build event" добавить в Main проект (сейчас он в InjectedLogic), возможна ситуация при которой Main не будет сбилджен т.к. в нем не было изменений, но будет вызван "post-build event", при этом билд InjectedLogic запустится уже после него, 
        т.е. будет скопирован старый InjectedLogic до того как сбилдится новый
 * "EnableDynamicLoading" property в .csproj проекта InjectedLogic позволит для него при компиляции сгенерировать InjectedLogic.runtimeconfig.json
   "CopyLocalLockFileAssemblies" позволит скопировать все Nuget зависимости в выходную папку bin
 * Reloaded.Memory.Sigscan 3.1.8 зависит минимум от Reloaded.Memory 9.1.0
   Reloaded.Hooks 4.3.0 зависит от Reloaded.Assembly 1.0.14 кот. зависит от Reloaded.Memory.Buffers 2.0.0 кот. зависит минимум от Reloaded.Memory 7.0.0
   Проблема в том, что между Reloaded.Memory 7.0.0 и 9.1.0 произошли критические изменения, а после компиляции моего проекта в bin папку копируется только Reloaded.Memory 9.1.0, так что Reloaded.Hooks начинает использовать 9.1.0 но не находит классов что были в 7.0.0
   Проблему удалось решить путем использования Reloaded.Memory.Sigscan версии 3.1.1 вместо 3.1.8, т.к. Reloaded.Memory.Sigscan 3.1.1 использует минимум Reloaded.Memory 4.1.4 и также с Reloaded.Memory 7.0.0
   + PackageReference подход для Nuget пакетов в .NET Core старается копировать те версии зависимостей которые подойдут везде (поэтому и копируется Reloaded.Memory 9.1.0), в моем же случае это не помогает ввиду критических изменений между версиями 
   + Такой вариант "Nuget version conflict" называется "Cousin dependencies": https://learn.microsoft.com/en-us/nuget/concepts/dependency-resolution#cousin-dependencies
 * В версии Reloaded.Hooks 4.3.0 больше нет Utilities.PushCdeclCallerSavedRegisters() и Utilities.PopCdeclCallerSavedRegisters(), вместо них есть ReloadedHooksUtilities.Instance.PushCdeclCallerSavedRegisters() и ReloadedHooksUtilities.Instance.PopCdeclCallerSavedRegisters()
   Однако они возвращают push и pop 3 регистров только для x86, поэтому для x64 требуется написать ассемблерные строки вручную используя x64 регистры
   
 * Вместо Named Pipes решено было использовать TCP соединение в виду его простоты и достаточной скорости. Сравнение скоростей: https://www.baeldung.com/linux/ipc-performance-comparison
 * Для отлова нажатия клавиш в консоли сначала использовался WinAPI метод GetAsyncKeyState(), он не блокирует работу консоли (и другие потоки в т.ч.) как это делает Console.ReadKey()
   Однако затем дабы не использовать Thread.Sleep() решено было использовать ManualResetEvent которого ждет основной консольный поток, нажатие Ctrl+C включает ManualResetEvent тем самым разрешая консольному потоку выполниться
   + В теории перед Console.ReadKey() можно использовать проверку "if (Console.KeyAvailable)" и вызывать Console.ReadKey() только при успешной проверке, однако Console.ReadKey() от этого не перестает блокировать консоль, а также пользователдю приходится дважды нажимать клавишу
   находясь в консоли (первый раз для прохождения проверки, второй для Console.ReadKey())
 * Console.WriteLine() в теории может не гарантировано работать с разными Thread-ами одновременно пишущими в консоль, доп. см.: https://github.com/dotnet/runtime/issues/53751
 * Пока что лишь класс DataToSendSyncKeeper кэширует строки. А при достижении максимального установленного кол-ва освобождает их
 
 * Вся методы что inject-ятся в игру на самом деле будут вызваны в отдельном потоке от основного игрового потока (т.к. они связаны с диалогами, а для диалогов у игры отдельный поток), поэтому они фактически не будут влиять на производительность.
   При дебаге Rider-ом остальные потоки продолжают работать, а при дебаге в Cheat Engine, он ставит на паузу все потоки при дебаге одного
*/

/* Нюансы реализации TCP сервера и клиента
 * Для сервера используется отдельный созданный поток, он ждет получения события (а именно DataSender ждет) с пом. AutoResetEvent и затем уже отправляет данные. Такой подход должен работать быстрее чем назначение Task-ов потокам из ThreadPool-а (т.е. при асинхронном вызове).
   Клиент же при этом использует асинхронный подход (т.е. его логика вызывается из потоков ThreadPool-а).
 * В случае если сервер внезапно умирает и не закрывает соединение с клиентом вызвав к прим. метод Close или Dispose по client и socket (или не используя using по ним), если сразу после этого клиент вызывовет блокирующий метод Read(), тогда он может
   продолжительное время (видимо бесконечно) висеть и ждать данных (в случае если у него не выставлен timeout На чтение). Если сервер закрывает соединение подобающим образом, а клиент в это время находится в блокирующем методе Read(), тогда Read() просто вернет 0.
   Доп. сведения о том когда именно Read() возвращает 0: https://learn.microsoft.com/en-us/archive/msdn-technet-forums/608f7100-2ee9-4e49-affd-bf3277579cfd и https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.read?view=net-8.0
 * Данные отправленные сервером не имеют гарантий быть полученными со стороны клиента, это нужно подтверждать вручную, доп. читать: https://stackoverflow.com/questions/30703500/does-tcpclient-write-method-guarantees-the-data-are-delivered-to-server?rq=1
 * Read() метод можно также использовать для ожидания поступления данных вместо stream.DataAvailable и Tread.Sleep (однако это не безопасный способ как описано выше): https://stackoverflow.com/questions/13097269/what-is-the-correct-way-to-read-from-networkstream-in-net
 * На данный момент сервер не подразумевает возможность получения пакета данных не с начала (т.е. с середины к прим.), такую логику с чтением header-а и уже последующим парсингом всего тела нужно реализовывать иным образом отличным от текущего,
   а именно читать все пришедшие данные в буффер, а затем уже парсить этот буффер в поисках header-а, размера пакета и самого тела. Пример реализации см. в nuget пакете NetCoreServer, а именно source code имплементацию HTTP клиента
*/

/* Нюансы WPF части
 * ZoomControl взят из оригинального репозитория GraphShape, а именно подпроекта GraphShape.Sample
 * ZoomControl в оригинале все свои стили определяет в ZoomControl.Resources.xaml, при этом ZoomControl.xaml файла нет, поэтому по умолчанию никто не ссылается на стили (сослаться можно было бы как в ZoomControl.cs так и в ZoomControl.xaml)
   В оригинальном GraphShape.Sample файл ZoomControl.Resources.xaml используется в Generic.xaml, он обязан лежать в папке Themes в корне проекта и будет использоваться WPF для дефолтного поиска стилей. При этом в Properties/AssemblyInfo.cs должен использоваться 
   специальный атрибут ThemeInfo дабы этот Generic.xaml использовался WPF. А в моем проекте путь в Generic.xaml должен быть прописан к прим. в виде: <ResourceDictionary Source="DialogGraph;component/View/ZoomControl/ZoomControl.Resources.xaml" />
   Доп. читать: https://stackoverflow.com/questions/1228875/what-is-so-special-about-generic-xaml
   Вместо этого подхода решено было ссылаться на ZoomControl.Resources.xaml используя абсолютный вида: <ResourceDictionary Source="pack://application:,,,/DialogGraph;component/View/ZoomControl/ZoomControl.Resources.xaml" />
   При этом абсолютный путь кот. прописан в примерах у Microsoft не сработал, сработала комбинация "pack://application:,,," и "/DialogGraph;component/...", 
   Доп. читать: https://stackoverflow.com/questions/22289367/pack-application-resourcefile-xaml-never-works и https://learn.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf?view=netframeworkdesktop-4.8
 * Rider ругался на огромное кол-во аллокации памяти (от пары сотен мб до 20 гб в зависимости от кол-во прокручиваний колесика мыши) выделяемой и сразу собираемой как мусор GC при прокручивании Zoom-а колесиком мыши.
   Оказалось что большое кол-во памяти выделялось при постоянной упаковке/boxing-е double, особенно с учетом того, что при Zoom-е работают еще 2 доп. анимации перемещения по оси X и Y.
   Сначала решено было сэкономить хотя бы на аллокации новой анимации каждый раз при каждом новой прокрутке колескика, а именно просто использовать уже запущенную анимацию и устанавливать туда новый "To", однако оказалось что WPF анимации 
   используются как Freezable объекты и не подразмевают изменений после запуска. Устанавливать новый double в старый объект аналогично не получилось, все доп. комментарии читать в Backup-е кода double анимации
   Поэтому решено было сделать свой собственный простой аниматор и анимацию. Они не поддерживают blending одной анимации в другую с пом. "HandoffBehavior.Compose", а также не исправляют полностью проблему необходимости постоянной упаковки double,
   однако анимация все равно потребляет гораздо меньше памяти и работает быстрее (доп. о принципе работы blending-а стандартной WPF анимации с пом. "HandoffBehavior.Compose" читать отдельный комментарий).
 * По умолчанию Class library project не может иметь App.xaml, добавление файла вручную будет выдавать ошибку "error MC1002: Library project file cannot specify ApplicationDefinition element", дабы от этого избавиться нужно
   в .csproj файле использовать PropertyGroup в виде <EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition> 
 * Текст в TextBox из кода можно заполнять путем обычного binding-а, однако если речь идет о постоянном дополнении новых строк в TextBox, то лучше использовать метод AppendText(). Однако метод AppendText() по TextBox-у нужно вызывать во ViewModel, а для этого
   в него нужно передать либо ссылку на сам TextBox (что плохо, т.к. ViewModel будет зависить от Control-а), либо передать делегат (вызывающий TextBox.AppendText()) во ViewModel (в таком случае ViewModel не будет зависить от Control-а, однако нужна специальная фабрика
   т.к. MainWindow зависит от VideModel, а ViewModel требует делегата от MainWindow - фактически Circular dependency). Изначально такой подход и был реализован, однако позднее решено было отказаться (Backup кода см. ниже)
   P.S. решено было не использовать Mediator, т.к. View и ViewModel лежат в одном слое, им не нужна слишком серьезная абстракция
   + (От этого варианта позднее отказался, см. Backup ниже) Дабы решить проблему сначала решено было использовать обычный Binding-а и кастомный Converter. У TextBox элемента в xaml есть MultiBinding (первый binding на себя и второй на текстовое свойство во ViewModel). 
     Когда текстовое свойство во ViewModel обновляется (там соотв. доп. вызывается OnPropertyChanged), срабатывает TextBoxAppendTextConverter, 
     и первым аргументом он получает ссылку на сам TextBox, а вторым строчку из свойства ViewModel, затем он просто вызывает метод AppendText() и возвращает спец. объект Binding.DoNothing дабы в TextBox не был полностью перезаписан текст.
     Это нестандартное использование Converter-а (обычно они должны ковертировать данные и возвращать их в Control), однако такой подход позволяет отделить ViewModel от TextBox-а
     P.S. если нужно выставить TwoWay Binding, нужно в <Binding RelativeSource="{RelativeSource Self}" /> либо дописать Mode="OneWay", либо Path="."
     Информация про Binding метода с использованием кастомного Covernter-а: https://stackoverflow.com/questions/502250/bind-to-a-method-in-wpf
     Информация про то как Converter-у получить ссылку на его же элемент: https://stackoverflow.com/questions/5454726/can-a-wpf-converter-access-the-control-to-which-it-is-bound
   + (Не применялся) Еще одним вариантом будет вместо кастомного Converter-а использование своего класса Control-а наследующегося он желаемого, к прим. TextBox-а. Кастомный Control будет иметь свое свойство в кот. можно будет сделать Binding к свойству во ViewMode,
     и при обновлении свойства в кастомном Control-е, он просто будет вызывать желаемый метод, к прим. AppendText() 
   + От Binding-а и кастомного Coverter-а решено было отказаться, т.к. подобных методов Control-ов которые необходимо вызывать из ViewModel может быть много, и для каждого тогда придется делать свой кастомный Converter
     Поэтому решено было сделать проще, в классе MainWindow есть nested MainWindowActions класс, он инжектится во ViewModel, при этом MainWindow в конструкторе устанавливает ссылку себе в приватное static поле, т.к. MainWindowActions nested, он без проблем может
     обращаться к private членам класса MainWindowActions. Гипотетически приватное static поле в MainWindow может быть не проинициализировано, при этом MainWindowActions может к нему обратиться, однако логически в такой ситуации нет никакого смысла, 
     т.к. выходит что какой-то класс пытается вызвать метод MainWindowActions, т.е. фактически метод какого-либо UI элемента (т.е. Control-а), при этом окно (MainWindow) этих UI элементов не создано/проинциализировано и не открыто
 * Presentation слой и Application слой общаются через Mediator. Благодаря такому подходу к прим. удалось решить проблему Circular dependency при: TCPClient требовал Logger, Logger требовал ViewModel, ViewModel требовал
   TCPClient (чтобы установить isLogEnabled, однако теперь этот bool держится прямо в Config-е). Внутри же одного слоя (в Presentation и в Application) классы общаются посредствам интерфейсов, как и Application с Infrastructure.
   Не смотря на использование Mediator, в проекте есть классы которые должны начать работу в строгом порядке, к прим. ConfigManager должен быть проинциализирован самым первым, а Logger должен начать работу после ViewModel (и вообще после того как MainWindow покажется) 
   т.к. Logger должен при своей инициализации передать во View превичный лог. В теории эту проблему можно решить обычным event-ом (в Mediator) из ViewModel (или MainWindow) на кот. подпишется Logger. Сейчас порядок разрешается в App.xaml.cs т.к. так проще и нагляднее 
 * Библиотека GraphShape предоставляет Control в виде Generic класса GraphLayout для отображения графа во WPF. Дабы использовать такой класс нужно унаследовать от него не Generic класс и в Generic параметры GraphLayout прокинуть конкретные TVertex, TEdge, TGraph
   Все три параметра - ViewModel классы объекты кот. будут созданы в Application слое. Выходит что CustomGraphLayout класс используемый во View (как Control) зависит напрямую от ViewModel классов. Если бы библиотечный GraphLayout принимал 
   Vertex-ы, Edge-и и Graph-ы с пом. binding-а, тогда бы проблем не было. Поэтому сейчас дабы абстрагировать View от конкретного GraphLayout класса, приходится использовать GraphLayoutCreatorExtension и GraphLayoutFactory 
 * CustomGraph можно сделать Generic, но с ним не получится работать используя абстракцию. Т.е. объект не получится cast-ить в переменную типа BidirectionalGraph<IGraphVertex, IEdge<IGraphVertex>> или в IMutableBidirectionalGraph<IGraphVertex, IEdge<IGraphVertex>>,
   т.к. для этого нужна ковариантность для Generic параметра IEdge в интерфейсе IMutableBidirectionalGraph (которой в нем соотв. нет), а в классах в принципе в C# нельзя использовать ковариантность. Поэтому приходится CustomGraph использовать как не Generic класс,
   а необходимые тесты желающие подменить CustomGraph на какой-либо другой класс должны будут просто сделать его наследника и использовать уже его
 * Два класса, а именно DialogProcessor, LocalizationFileParser - кэшиируют в большом кол-ве необходимые данные, а затем достигнув определенного пика по памяти, очищают их. Доп. комментарии см. в этих классах
   При этом больше всего памяти аллоцирует библиотека при построении графа. Однако эта память собирается GC в т.ч. когда строится новый граф (и старый соотв. уходит в утиль).
   Есть и другие классы использующие к прим. постоянные List-ы или StringBuilder, к прим. DialogJsonParser или CommonHelpers::JoinSpan(), но там обычно всегда небольшие размеры по памяти
 * Стоит заметить, что данный проект не является идеальным видом DDD, т.к. основная бизнес-логика лежит в Application слое, а не в Domain слое. А Infrustructure слой создает экземпляры Domain объектов вручную, тем самым зависит от них (в идеале конечно
   нужно создавать экземпляры классов с пом. одного обычного вызова JsonSerializer.Deserialize()). Так что текущее архитектурное решение больше напоминает Clean Architecture



 * Принцип работы установки X и Y позиций для Vertex и Edge элементов GraphLayout класса.
   Изначально когда объект CustomGraph ставится в DependencyProperty Graph класса GraphLayout<TVertex,TEdge,TGraph> (которого сейчас наследником является класс CustomGraphLayout), вызывается метод OnGraphPropertyChanged(), кот. вызывает OnRelayoutInduction(),
   далее происходит просчет всего layout-а графа, а в самом конце вызывается метод ChangeState() кот. вызывает ApplyVerticesPositions(), и он уже ставит новые позиции через аниматор (если он есть). Аниматор ставить позиции (в течении небольшого времени)
   путем вызова методов SetX() и SetY() класса GraphCanvas (базовый класс для класса GraphLayout). SetX() и SetY() методы ставят DependencyProperty XProperty и YProperty соответственно, а те благодаря битмаске Enum-ов при инициализации: 
   FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange -
   вызовут переопределенные в классе GraphCanvas методы ArrangeOverride() и MeasureOverride(). ArrangeOverride() - вызывает GetX() и GetY() методы по всем Control-ам и устанавливает им новую позицию, а MeasureOverride() - аналогично устанавливает размер GraphCanvas-а
 * Принцип работы HandoffBehavior.Compose анимации
   Запуск сначала первой, а затем второй анимации (до того как первая закончилась) с пом. "BeginAnimation(dp, doubleAnimation, HandoffBehavior.Compose)" с аргументом "HandoffBehavior.Compose" фактически будет выполнять две анимации друг за другом.
   При этом вторая анимация в свое начальное значение (из кот. нужно делать интерполяцию в конечное) будет получать результат интерполяции первой анимации. К прим. первая анимация интерполирует 1 в 10, а вторая - получает от первой результат (к прим. 8) и 
   интерполирует его к к прим. 17, у обоих анимаций одинаковая продолжительность. В тот отрезок времени пока работает одновременно первая и вторая анимация (т.е. первая не успела еще закончится), интерполированное значение будет набираться быстрее за счет двойной интерполяции. 

*/

/* Доп. возможности WPF и пр.
 * В Json.Net (Newtonsoft.Json) в классе JsonTextReader есть свойство ArrayPool - позволяет оптимизировать по памяти

 * В preview визуала в Rider можно переключаться между вкладками TabControl, надо лишь правильно нажать именно на нужный Tab в preview, а не на текст в нем прописанный
 * С пом. класса DispatcherTimer можно использовать Message Loop WPF приложения чтобы с определенной периодичностью вызывать любые функции в основном потоке WPF
   Доп. о DispatcherObject читать: https://professorweb.ru/my/WPF/documents_WPF/level31/31_2.php
 * Пример binding-а метода к TextBox-у, однако он видимо срабатывает только при событии от TextBox-а к методу но не обратно: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-bind-to-a-method?view=netframeworkdesktop-4.8
 * Возможность с пом. VisualTreeHelper класса собрать все/любые скрытые (внутренние из Template) элементы из любого Control-а: https://stackoverflow.com/questions/10279092/how-to-get-children-of-a-wpf-container-by-type
 * Возможность с пом. LogicalTreeHelper класса собриать все/любые вложенные элементы любого Control-а: https://professorweb.ru/my/WPF/Template/level17/17_2.php
 * Возможность из Template любого Control-а взять внутренний элемент по имени с пом. myControl.Template.FindName(...): https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-find-controltemplate-generated-elements?view=netframeworkdesktop-4.8
 * Возможность создавать внешние дополнительные (не определенные в оригинальном Control-е) Attached Property для любых Control-ов и кастомную логику с пом. наследника класса Behavior (нужен nuget Microsoft.Xaml.Behaviors.Wpf): https://habr.com/ru/articles/254887/
   Также существуют и Attached event-ы: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/events/attached-events-overview
 * DataTemplate можно использовать не только для определения вида каких-либо данных (не Control-ов) для их визуализации, но также и обычных Control-ов, а также DataTemplate можно определить внутри ContentTemplate элемента не вынося DataTemplate в Resources блок,
   Основное описание DataTemplate и в т.ч. DataTemplateSelector: https://professorweb.ru/my/WPF/binding_and_styles_WPF/level20/20_4.php
   Простой пример динамической подмены одного Control-а на другой в т.ч. с пом. DataTemplate: https://ru.stackoverflow.com/questions/1485396/wpf-mvvm-Переключение-content-у-contentcontrol 
 * MarkupExtension - базовый класс наследники кот. могут вызываться внутри xaml как этакие функции-операторы (к прим. как binding или StaticResource) с возможностью передачи им аргументов: https://habr.com/ru/articles/84263/

*/

/* TODO:
 * Если TCP клиент перезагрузился, тогда сервер должен заного отправить клиенту все закэшированные name + guid
 * Flags to check и set в графе при наведении на них мышкой должны подствечивать самих себя но уже в других нодах дабы было видно какие ноды работают (читают или ставят) с одинаковыми флагами
 * Возможно имеет смысл сделать опцию при включении которой, когда активным становится новый диалоговый нод (т.е. пользователь нажимает на какую-либо опцию диалога), зум графа делает приближение (скорее перемещение) в строну этой активной ноды так, чтобы она к прим. оказалась в центре 
 * Маленькое окошко показывающее данные (в виде текста) node-ов нужно сделать расширяемым и возможно более комплексным по функционалу (к прим. сделать табы, дабы можно было посмотреть отдельно данные по нодам и флагам, добавить цвет, возможно поиск)
 * Node типа PersuasionResult в зависимости от удачного/неудачного типа имеет смысл красить в разный цвет (пока что различается лишь текст, в Dialog Editor в SDK окрашивается border удачного и неудачного Node-а в зеленый и красный цвет соотв.)
 * В теории можно сделать открывающееся окошко с какими-либо доп. опциями по нажатию правой кнопки мыши на любой node  
 * В теории можно сделать мини-карту
 * Разобраться как работает расчет статистики для убеждения в диалогах. Т.к. в файлах диалогов у Persuasion нодов есть ключ "DifficultyMod", однако это не тот уровень что на самом деле просится в диалогах при убеждении.
   Более того, в Dialog Editor в SDK у Persuasion нодов не показывается уровень сложности убеждения. Судя по всему игра его высчитывает динамически. Вероятно в т.ч. используя ключ "DifficultyMod" и "LevelOverride"
 * В GraphShape нет опции layout-а edge-ей по принципу "Polyline Edge Routing" в Yworks: https://docs.yworks.com/yfiles-html/dguide/layout/polyline_router.html
   Или по принципу "Orthogonal" в GraphViz (в GraphShape есть опция Orthogonal Routing, однако она Edge-и merge-ит в один, а нужно чтобы они были разлельными): https://graphviz.org/docs/attrs/splines/
   В GraphShape в т.ч. за layout edge-ей отвечает класс SugiyamaLayoutAlgorithm. Гипотетически его можно полностью скопировать и переделать алгоритм Edge-ей на желаемый вручную.
   Альтернативой можно в принципе использовать GraphViz для просчета layout-а а тут лишь выводить конечные UI Элементы
   P.S. в каждом диалоговом файле в каждой ноде (в json) есть json ключ "editorData", он в т.ч. хранит позиции нодов в для отображения нодов в Dialog Editor в SDK, их можно использовать вместо автоматического layout-а,
   однако тогда нужно вручную создавать все vertex-ы и edge-и по желаемым позициям
 */

// BackUp-ы примеров кода:
#region BU старого кода проверки нажатия клавиш в косноле с пом. GetAsyncKeyState()
enum PressedKeys
{
  GKey = 0x47,
  Ctrl = 0x11,
}

public void RunConsole()
{
  while (true)
  {
    Thread.Sleep(100);
            
    var wasPressed = WinAPIWrapper.GetAsyncKeyState((int)PressedKeys.GKey);
    var ctrlWasPressed = WinAPIWrapper.GetAsyncKeyState((int)PressedKeys.Ctrl);
    if(wasPressed == false || ctrlWasPressed == false) continue;

    // ...
  }
}


#endregion
#region Пример инъекции delegate-а
/*
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int AddFunction(IntPtr a);
private static IAsmHook? _addAfterOriginalHook;
public static int Add(IntPtr a)
{
    Console.WriteLine("Delegate was executed: " + a);
    return 11259375;  
}
private static IReverseWrapper<AddFunction> _reverseWrapAddFunction;


string delegatePointer = Utilities.GetAbsoluteCallMnemonics(Add, out _reverseWrapAddFunction);
string[] functionsBytesToInject = 
{
    $"{_use32}",
    $"push {_eax}",
    $"push {_ecx}",
    $"push {_edx}",
    $"push {_ebx}", // проброс аргумента
    $"{delegatePointer}",
    $"call {_eax}",
    $"pop {_edx}",
    $"pop {_ecx}",
    $"pop {_eax}",
};
var tempAddr = baseAddress + pointerOfTESTFUNCTION.Offset;
_addAfterOriginalHook = ReloadedHooks.Instance.CreateAsmHook(functionsBytesToInject, (long)tempAddr, AsmHookBehaviour.ExecuteAfter).Activate();
*/

#endregion
#region Пример реализации FixedString с помощью StructLayout атрибута
[StructLayout(LayoutKind.Explicit, Size = 18)]  // размер структуры аналогичен текущей использующейся реализации FixedString класса (использующей fixed-size buffer), т.е. равен 18 (на Length приходится 2 байта вместо одного видимо ради data align-а)
public struct TEST_FixedString_With_StructLayoutAttribute
{
  [FieldOffset(0)]
  public byte firstByte;
    
  [FieldOffset(16)]
  public byte Length;
    
  public unsafe void WriteByte(byte b, byte index)
  {
    fixed (byte* firstBytePointer = &firstByte)
    {
      firstBytePointer[index] = b;
    }
        
    /* Сравнение нескольких подходов получения указателя на поле в структуре
    // Компилятор требует использования "fixed" оператора т.к. если мы положим структуру в поле какого-либо класса, а затем вызовем этот метод путем SomeClass.ThisStruct.WriteByte(), в таком случае этот метод получит ссылку на структуру в куче (Heap) а не на стеке
    // Так что использование Unsafe.AsPointer() метода небезопасно в таком случае, однако безопасно если структура гарантировано лежит в стеке в  методе кот. вызвал текущий метод
    // Комментарии об адресах ниже написаны с учетом того, что структура лежит в стеке
    var testPointer1 = (byte*)Unsafe.AsPointer(ref firstByte);
    Console.WriteLine((long)testPointer1);
    
    //fixed(byte* tete2 = &firstByte)   // возвратит тот же адрес что и "fixed" ниже
    fixed(TEST_FixedString_With_StructLayoutAttribute* testPointer2 = &this)
    {
        Console.WriteLine((long)testPointer2);      // выведет тот же адрес что Unsafe.As() выше
    }*/
        
  }
}
#endregion
#region Старый подход при котором MainWindow получал в конструктор ссылку на MainWindowViewModelInitializer, вызывал по нему метод Initialize(arg) кот. инициализировал mainWindowViewModel
// У этого кода есть альтернатива при которой MainWindowViewModelInitializer (кот. уже будет singleton фабрикой в таком случае) будет сам создавать экземпляр класса MainWindowViewModel, в остальном отличий в общем не будет
// В CreateIOCContainer() методе
var mainWindowViewModel = new MainWindowViewModel();
IOCContainer.MainWindowViewModelInitializer = new MainWindowViewModelInitializer(mainWindowViewModel);
IOCContainer.MainWindow = CreateThreadUnsafeLazyObj(() => new MainWindow(IOCContainer.MainWindowViewModelInitializer));

IOCContainer.Logger = CreateThreadUnsafeLazyObj(() => (ILogger)new LoggerViewModel(IOCContainer.MainWindowViewModelInitializer.Instance));

// В MainWindow
public MainWindow(MainWindowViewModelInitializer mainWindowViewModelInitializer)
{
  InitializeComponent();

  this.mainWindowViewModel = mainWindowViewModelInitializer.Initialize(AppendTextToLog);
  DataContext = mainWindowViewModel;
}

private void AppendTextToLog(string text)
{
  this.LogTextBox.AppendText(text);
}

// В MainWindowViewModel
public MainWindowViewModel Initialize(Action<string> writeLogText)
{
  this.writeLogText = writeLogText;
  return this;
}

// В файле Factories.cs
public abstract class BaseInitializer<T>
{
  protected T obj;
  protected bool isInit;

  public T Instance
  {
    get
    {
      if (isInit == false) throw new InvalidOperationException($"Trying to get the {typeof(T).FullName} class before it is initialized");
      return obj;
    }
  }
    
  protected void ThrowIfReinitializationHappens()
  {
    if (isInit) throw new InvalidOperationException($"An attempt to initialize an already initialized class {typeof(T).FullName}");
  }

  protected T EndInit()
  {
    isInit = true;
    return obj;
  }
}

public class MainWindowViewModelInitializer : BaseInitializer<MainWindowViewModel>
{
  public MainWindowViewModelInitializer(MainWindowViewModel obj) => this.obj = obj;

  public MainWindowViewModel Initialize(Action<string> writeLogText)
  {
    ThrowIfReinitializationHappens();
    obj.Initialize(writeLogText);
    return EndInit();
  }
}

#endregion
#region Старый подход при котором MainWindowViewModel имел свойство TextForLog, а TextBox в xaml имел MultiBinding и кастомный Converter который вместо того чтобы просто возвращать текст в TextBox, вызывал его метод AppendText() 
// В MainWindowViewModel
private string textForLog;
public string TextForLog
{
  get => textForLog;
  set
  {
    textForLog = value;
    OnPropertyChanged();
  }
}

// В MainWindow.xaml
<Window.Resources>
  <local:TextBoxAppendTextConverter x:Key="TextBoxAppendTextConverter"/>
</Window.Resources>

  <TextBox x:Name="LogTextBox"
Background="#012456"
Foreground="White"
TextWrapping="Wrap"
IsReadOnly="True"
VerticalScrollBarVisibility="Auto">
  <MultiBinding Converter="{StaticResource TextBoxAppendTextConverter}" Mode="OneWay">
  <Binding RelativeSource="{RelativeSource Self}"/>
  <Binding Path="TextForLog"/>
  </MultiBinding>
</TextBox>
  
// В TextBoxAppendTextConverter
public class TextBoxAppendTextConverter : IMultiValueConverter
{
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
  {
    var control = values[0];
    if (control is not TextBox textBox) throw new ArgumentException("This converter should only be used for TextBox Control");
    var argument = values[1];
    if (argument != null)
    {
      textBox.AppendText(argument.ToString());
    }
        
    return Binding.DoNothing;
  }
    
  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException("This converter should only be used for 'One way binding'");
  }
}

#endregion
#region Кастомная double анимация в виде наследника стандартного DoubleAnimation
// Наследник DoubleAnimation для удобного дебага
// Доп. о неизменяемости свойств из-за Freezable объекта читать комментарии к методу CreateInstanceCore()
public class TestDoubleAnimationChild : DoubleAnimation
{
  private double to;
  public TestDoubleAnimationChild(double toValue, Duration duration) : base(toValue, duration)
  {
    to = toValue;
  }
    
  protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
  {
    double progress = animationClock.CurrentProgress.Value;
    double from = defaultOriginValue;
    Console.WriteLine(Environment.NewLine + "Progress: " + progress);
    Console.WriteLine("From: " + from);
        
    Console.WriteLine("To prop: " + To);
        
    //var result = from + (to - from) * progress;
    //Console.WriteLine("Result: " + result);
        
    var referenceResult = base.GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
    Console.WriteLine("ReferenceResult: " + referenceResult);
        
    return referenceResult;
  }

  // При вызове метода BeginAnimation() спустя н-ное кол-во вызовов класс AnimationTimeline вызовет метод AllocateClock кот. создаст новый Clock а тот у себя в конструкторе через пару методов вызовет этот CreateInstanceCore()
  // Таким образом логика создавшая экземпляр этого класса будет иметь другой экземпляр класса нежели логика WPF аниматора который будет вызвать здешний GetCurrentValueCore()
  // Поэтому логика приложения не может обновить To, From и пр. свойства, т.к. экземпляр класса другой
  // Даже если здесь вернуть не новый объект, а "this", последующая установка нового значения к прим. в "DependencyProperty To" - вызовет исключение
  protected override Freezable CreateInstanceCore()
  {
    return new TestDoubleAnimationChild(to, Duration);
  }
}
#endregion
#region Кастомная double анимация в виде наследника базового AnimationTimeline 
/* Данный класс использовался дабы полностью избежать использования стандартного класса DoubleAnimation и его базового класса DoubleAnimationBase для получения полного контроля над double значением и избежания проблем Freezable объекта в оригинале 
 * От этого варианта пришлось отказаться т.к. AnimationStorage.GetCurrentPropertyValue() вызывающий здешний GetCurrentValue() ждет от него новый object с новым double, поэтому установка нового double значения в старый object
   с пом. "CommonHelpers.SetBoxedDoubleWithoutAllocation(ref defaultOriginValue, result)" нарушал логику наботы AnimationStorage.GetCurrentPropertyValue(), тем самым ломал анимацию и анимация переставала работать полностью
 * При этом WPF логика всегда вызывает CreateInstanceCore() возвращающий Freezable объект (к прим. при вызове метода BeginAnimation из ZoomControl).
   Не смотря на то, что можно возвращать "this" вместо нового экземпляра класса (тогда у ZoomControl будет тот же экземпляр класса что и у AnimationStorage), такой подход нестандартен, проще сделать свой собственный легковесный аниматор и анимацию с доп. оптимизацией
 */
// Класс сделан на основе DoubleAnimationBase, многие элементы удалены в виду отсутствии необходимости
public abstract class BaseTestCustomDoubleAnimation : AnimationTimeline
{
    public override sealed object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
    {
        // Verify that object arguments are non-null since we are a value type
        if (defaultOriginValue == null)
        {
            throw new ArgumentNullException("defaultOriginValue");
        }
        if (defaultDestinationValue == null)
        {
            throw new ArgumentNullException("defaultDestinationValue");
        }

        double defaultOriginValueDobule = (double)defaultOriginValue;
        double defaultDestinationValueDouble = (double)defaultDestinationValue;
        double result = GetCurrentValue(defaultOriginValueDobule, defaultDestinationValueDouble, animationClock);
        return result;

        // Код устанавливающий новый double в старый object без аллокации, доп. читать комментарии в шапке
        //if (defaultOriginValueDobule.Equals(result) == false) CommonHelpers.SetBoxedDoubleWithoutAllocation(ref defaultOriginValue, result);
        //CommonHelpers.SetBoxedDoubleWithoutAllocation(ref defaultOriginValue, result);
        //return defaultOriginValue;
    }
    
    public Double GetCurrentValue(Double defaultOriginValue, Double defaultDestinationValue, AnimationClock animationClock)
    {
        ReadPreamble();

        if (animationClock == null)
        {
            throw new ArgumentNullException(nameof(animationClock));
        }

        if (animationClock.CurrentState == ClockState.Stopped)
        {
            return defaultDestinationValue;
        }

        return GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
    }

    public override Type TargetPropertyType
    {
        get
        {
            ReadPreamble();

            return typeof(Double);
        }
    }
    
    protected abstract Double GetCurrentValueCore(Double defaultOriginValue, Double defaultDestinationValue, AnimationClock animationClock);
}


public class TestCustomDoubleAnimation : BaseTestCustomDoubleAnimation
{
    
    public TestCustomDoubleAnimation(double toValue, Duration duration) : base()
    {
        to = toValue;
        Duration = duration;
    }

    private double to;
    protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
    {
        double progress = animationClock.CurrentProgress.Value;

        double from = defaultOriginValue;
        Console.WriteLine(Environment.NewLine + "Progress: " + progress);
        Console.WriteLine("From: " + from);
        
        var result = from + (to - from) * progress;
        Console.WriteLine("Result: " + result);

        return result;
    }
    
    // Про Freezable читать комментарии в шапке класса BaseTestCustomDoubleAnimation
    protected override Freezable CreateInstanceCore()
    {
        return new TestCustomDoubleAnimation(to, Duration);
    }
}


#endregion








