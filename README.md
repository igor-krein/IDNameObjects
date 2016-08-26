# IDNameObjects
C# Extensions API for Entity Framework intended to simplify usage of drop-down lists, comboboxes and alike

## What this all is about
When you work with something like drop-down list, in most cases, all you actually need is a list of ID+Name pairs. These extension methods implement typical tasks of ID+Name logic and could drastically reduce the code.

Let's start from the end. If you are web developer you probably know that sometimes you shouldn't populate select-list with all the possible values, because there are a lot of them. To create a list with several thousands items when the user needs to choose only one means an excessive traffic and an excessive load on the database. The typical solution: to retrieve data partially, on demand. For example, user could start to type a name of the item and the web page automatically sends the ajax-request and gets some filtered data. The better solution is to break this filtered data on "pages": when the user scrolls list of already retrieved items to the end the web page automatically requests the next portion of data.

When we talk about JQuery, all this could be achieved with the help of the <a href="https://select2.github.io/">Select2</a>. On the server side, you should create a proper query to get the data requested, convert it to Select2-format and return the result.

Consider the following simple model.

    public class Book
    {
        public int ID { get; set; }
        
        public string Title { get; set; }

        // ...some more properties that are of no importance right now
    }
    
The method that gets the ajax request would probably look like this:

    [HttpGet]
    public ActionResult GetBooks(string search, int pageNumber, int pageSize)
    {
        // suppose the db variable is of a proper DbContext-derived class with the DbSet<Book> Books property
        var books = db.Books
                        .Where(b => b.Contains(search))       // filter the titles by substring
                        .OrderBy(b => b.Title)                // order titles by name
                        .Skip(pageSize * (pageNumber-1))      // go to the proper virtual "page"
                        .Take(pageSize)                       // take all the items from this page
                        .ToList();                            // load the result set to List
        // skip the rest for now
    }

As you can see in the example above, what we did is actually got the data from the DB - but not in Select2-format yet. You could write the method that converts the Book entity to Select2 object (it should have two properties: _id_ and _text_), or you could create an anonymous objects immediately before ToList() call - in any case you'll have to do something special. And when you are in the need to add the same logic for another model (say, Movie, or Actor, or Car - anything that has a name and could be identified by some key) - you'll have to repeat this code.

...Or you could simply write this:

    var select2result = db.Books.ToSelect2PageResult(search, pageNumber, pageSize);

In one line of code you got all the data ready to be sent back like this:

    return new JsonResult
    {
        Data = new { result = select2result },
        JsonRequestBehavior = JsonRequestBehavior.AllowGet
    };

Note: select2result variable is of type Select2PageResult which consists of a Select2-compatible list of _items_ and a boolean property named _more_. If _more_ is true that means there are more relevant data in DB (i.e. this is not the last page yet). This value could be used for Select2 pagination. In the _misc_ folder, there is a JavaScript function you could use for ajax requesting-responsing.
    
## How all this magic is done

Actually, I lied: you couldn't apply this magic to our Book model right away. You would - if the Title property was named Name instead. But you don't have to change your model just to fit it into this logic. You just have to adjust it a little, like this:

    public class Book
    {
        public int ID { get; set; }

        [IDNameObjectProperty("Name")]
        public string Title { get; set; }

        // ...some more properties that are of no importance right now
    }
    
Here. Now you can do the magic. IDNameObjects API will know now what to take as an ID and what - as a Name.

You can do even this:

    public class Book
    {
        [Key]
        public string ISBN { get; set; }

        [IDNameObjectProperty("Name")]
        public string Title { get; set; }

        // ...some more properties that are of no importance right now
    }

Note, that there is a support for IDNameObjectProperty("ID") attribute, but since we use Entity Framework and, in this case, we have to use Key attribute, that would be excessive.

Note also, that type of the [ID] property is string in the latter example, not an int. IDNameObjects API supports both (and, maybe, others as well - who knows). The [Name] property is always considered as a string, though.

And one more word, just to close the issue: IDNameObjects API would recognize an identificator named like BookID. But right now there is no support for such a naming for derived classes.

## Making select lists

Before all this Select2+Ajax thing, what we really need is to prepare an HTML select/option tags. In ASP.NET MVC there are SelectList and MultiSelectList classes that usually do the trick. And you could use them directly if you prefer. Alternatively, you could use IDNameObjects extensions.

Before showing you the examples, I have to remind you that we don't have to pass all the data to the client. All we need is to get currently selected item (or, in case of multi-select list, items) only. All the rest would be drawn on user's demand with the help of GetBooks method.

Let's start with [single-]select list (suppose we need to choose one favorite book only). Let's say, there is such a book already chosen (and stored in DB) and we want to let the user change his/her choice. So, we do this:

    Book book;
    // Here should be some code to get the current book from the DB
    ViewBag.BooksList = book.ToSelectList();

That's all. Really. I mean it.

Well, OK. Let's pretend we don't have a Book object yet and there is only a bookID (or, if you prefer, isbn) value. Remember the DbContext with DbSet&lt;Book&gt; property?

    ViewBag.BooksList = db.Books.ToSelectList(bookID);

If, for some reason, you want to create a complete list of options and, consequently, not to fetch data via ajax requests (say, the list is too short to bother), you could do this:

    ViewBag.BooksList = db.Books.ToSelectList(bookID, false);

With multi-select lists everything is pretty much the same (well, almost). So, we have to choose several (zero to all) books:

    // object reader is of class Reader that consists of ICollection<Book> FavoriteBooks navigational property
    ViewBag.BooksList = reader.FavoriteBooks.ToMultiSelectList();

    // if we have an IList of selected book identificators, i.e. int[] ids or List<int> ids
	// (in case of string ISBN key we would use IList<string>)
	ViewBag.BooksList = db.Books.ToMultiSelectList(ids);

	// the same, but with all items to choose from
	ViewBag.BooksList = db.Books.ToMultiSelectList(ids, false);

Now note the difference between reader.FavoriteBooks and db.Books. The first is ICollection, the second is IQueryable. And when you make this call...

	ViewBag.BooksList = db.Books.ToMultiSelectList();

...you'll get the complete list of books with no items selected (in case of ICollection you would get selected items only).

## What is inside

I suppose now you have some questions and I'll try to answer them. First of all...

### How all the lists are ordered

Up until now the default order was applyied everywhere where it is required. Default order is simply order by [Name] property, ascending. Surely, there are cases when such a behaviour is not appropriate. You have an option (even several options) to deal with it.

__First__, you could change the default order for the model. This could be a little tricky, but the good news it should be done only once.

Let's change our Book model a little:

    public class Book
    {
        public int ID { get; set; }

        [IDNameObjectProperty("Name")]
        public string Title { get; set; }

        public int Year { get; set; }

        // ...some more properties that are of no importance right now
    }
    
Suppose, we want the default order to be like this: first by year, and afterwards by title. We could do this by adding to our model a class method:

    [IDNameObjectMethod("OrderBySelector")]
    public static Expression<Func<T, object>> TypeHereAnyNameYouWant<T>()
        where T : Book
    {
        return b => b.Year.ToString() + b.Title;
    }

Note 1: You could name this method simply OrderBySelector and omit the attribute:

    public static Expression<Func<T, object>> OrderBySelector<T>()
        where T : Book
    {
        return b => b.Year.ToString() + b.Title;
    }

Note 2: Actually you could simplify this a little and don't use generic, like this:

    public static Expression<Func<Book, object>> OrderBySelector()
    {
        return b => b.Year.ToString() + b.Title;
    }

But if you plan to derive a new model from the Book class and use the same ordering method for it you should use a generic version.

Note 3: You are limited here with usage of expressions that are supported by LINQ. On the other hand, I don't see why you couldn't play with expression trees here if you like.

As you have noticed, here we actually combined two different order-by expressions in one. Maybe it is not very good from the DB point of view, so there is a __second option__, which is also more user-friendly:

    [IDNameObjectMethod("Order")]
    public static IQueryable<T> SomeMethod<T>(IQueryable<T> set)
        where T : Book
    {
        return set
            .OrderBy(b => b.Year)
            .ThenBy(b => b.Title);
    }

Here you could combine any EF Fluent API ordering extension methods.

But there is a drawback, though: default order defined in such a way won't work in IDNameObjects API methods that work with ICollection (maybe I'll fix this in the future).

__Third option__: use explicit ordering. Most of the API methods support optional lambda-expression parameter like this:

    var select2result = db.Books.ToSelect2PageResult(search, pageNumber, pageSize, b => b.Year.ToString() + b.Title);
	// ...
	ViewBag.BooksList = db.Books.ToMultiSelectList(ids, b => b.Year.ToString() + b.Title);

Explicit ordering could be useful in cases when your default ordering does not suit. For example, you want to show selected books in you multi-select list in one order, but when you compose your ajax response to fill the available options you'd like the items to be ordered another way.

And yes, here, once more, there is a limitation (one order-by ascending expression), but my guess it would be enough for most of the cases. But if there is a very special case, you could try __fourth option__, which is to combine IDNameObjects extension methods with the standard EF fluent API. I'll show you how to do this a little later.

### How the data is filtered

By default (implicitly), when you pass the string "search" parameter in IDNameObjects API methods, it means that the [Name] property should contain this parameter as a substring, case-insensitive. As an addition, when this search parameters "is null or empty", the filter returns _all_ the items instead of none. My guess, this behaviour is appropriate for most of the cases. But, of course, there are cases, and there are cases. So, how to customize filtering?

__First option__: switch default [Name] property filtering from String.Contains() method to String.StartsWith() method. It is achievable with the help of this class-level attribute:

    [IDNameObject(NameSearchType = "StartsWith")]
    public class Book
	{
		// ...
	}

Generally, there are only two these methods currently supported. But if, for some reason, you want to change default name filtering behaviour, you could define yours like this (__second option__):

    [IDNameObjectMethod("NameContainsPredicate")]
    public static Expression<Func<T, bool>> SomeMethod<T>(string filterString)
        where T : Book
    {
		// Check for strict equality, for example
        return b => b.Title.Equals(filterString);
    }

Another default filtering is applied to IList of [ID] property values. Naturally, it is "translated" to SQL "IN" operator (like "WHERE [ID] IN (23, 56, 4)"), which in C# terms means usage of List.Contains() method. Additionally, default filter method is smart enough to deal with special cases like:

- IList object is null (it won't throw an exception, but will result in false expression, i.e. return an empty result set);

- IList object is empty (the same);

- IList object consists of only one item (it would generate a simple equality expression, which, in my opinion, is preferable than IN operator).

But you have an option to change this behaviour manually adding to your model a proper method like this:

    [IDNameObjectMethod("IDsInListPredicate")]
    public static Expression<Func<T, bool>> SomeOtherMethod<T>(IList<int> ids)
        where T : Book
    {
	    // here you have to invent an alternative filtering
        return b => ids.Contains(b.ID);
    }

### What about composite naming

In many cases, a [Name] property would be not simple, but composite. Consider the following example:

    public class Person
    {
        public int ID { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }
        private string middleNamePart { get { return (MiddleName?.Length ?? 0) > 0 ? $" {MiddleName}" : ""; } }

        [Required]
        public string LastName { get; set; }

        [IDNameObjectProperty("Name")]
        public string FullName
        {
            get
            {
                return $"{FirstName}{middleNamePart} {LastName}";
            }
        }

        // plus some other properties
    }

Our [Name] property is combined from three other properties and, which is important, is not mapped to the database. It means, in the DB table (named Persons or maybe People), there are fields FirstName, MiddleName and LastName, but there is no field for FullName. In most cases, it is OK, but if you try to use FullName in LINQ expressions you'll get a run-time error. As a result, IDNameObjects API would fail to apply its filter and order methods which is annoying.

What do we do? Well, you already know how to customize default behaviour, so this seems an option. And it is - for a default order method. Something like this would do:

    public static Expression<Func<T, object>> OrderBySelector<T>()
        where T : Person
    {
		// here we chose to sort by last name first and then by first and middle names
        return p => p.LastName + " " + p.FirstName + " " + (p.MiddleName ?? "");
    }

The real pain in err... that important part of every developer's body is composite name filtering. It is hard enough to write a String.Contains() analog that works with several strings you aren't permitted to concatenate - even without all the LINQ limitations.

The alternate solution is to forse EF to create a calculated field (column), so the IDNameObjects API could use it on a DB level. There is a <a href="http://www.davepaquette.com/archive/2012/09/23/calculated-columns-in-entity-framework-code-first-migrations.aspx">little article</a> on how to achieve this.

In our example we could end up with something like this:

	// This is a new [calculated] property for our default filtering

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [IDNameObjectProperty("NameDB")]
    public string SearchName { get { return $"{FirstName} {LastName}"; } private set { } }

And don't forget that in your migration file you should add something like this:

    Sql("ALTER TABLE dbo.Persons ADD SearchName AS FirstName + ' ' + LastName");

Note that we skipped a MiddleName here for simplicity (suppose, we don't really want to search by middle names). Note also the IDNameObjectProperty("NameDB") attribute. It means that your model would actually have not only [ID] and [Name] properties, but also an additional [NameDB] property. The [Name] property is considered virtual and [NameDB] - actual DB data.

### What about composite keys support

No, there is no such thing yet. And very likely won't be.

### How to use fluent API

Up until now what we actually used is some high-level API methods (for creating select lists and for preparing data for Select2). These methods are belonged to Web API part (namespace IDNameObjects.Mvc).

But there are more (namespace IDNameObjects). Here are some examples.

#### QuickList methods

These methods use default filter and order methods and put the result in the list

    // QuickList for name property filter
	var selectedBooks = db.Books.QuickList(search);
	
	// the same - for IList of identificators
	var selectedBooks = db.Books.QuickList(ids);
	
	// search could be paginated
	var selectedBooks = db.Books.QuickList(search, pageNumber, pageSize);

	// or like this (default pageSize is determined by a class-level attribute like [IDNameObject(PageSize = 20)])
	var selectedBooks = db.Books.QuickList(search, pageNumber);

	// additionally you can specify alternate order expression
	var selectedBooks = db.Books.QuickList(search, pageNumber, b => b.Year.ToString() + b.Title);

Note: the selectedBooks variable would be of type IList&lt;Book&gt;.

#### QuickQuery methods

You could apply the filter, order and pagination (i.e. prepare a query) but not create a list yet, if you prefer:

    // QuickQuery for name property filter
	var selectedBooksQuery = db.Books.QuickQuery(search);
	
	// search could be paginated
	var selectedBooksQuery = db.Books.QuickQuery(search, pageNumber, pageSize);

	// ...etc.

Note: the selectedBooksQuery variable would be of type IQueryable&lt;Book&gt;, so you could use it, say, with standard EF fluent API.

#### Default filter/order methods. Pagination

The methods above are actually based on default filter (DefaultWhere) and order (DefaultOrder) methods, and also on a Page method. You, too, could use them.

    // Here is the sample of special order (mix of standard EF fluent API an IDNameObjects API)
	var selectedBooks = db.Books
	                        .DefaultWhere()
							.OrderByDescending(b => b.Year)
							.ThenBy(b => b.Title)
							.Page(pageNumber, pageSize)			// pages are numbered from 1 for convenience
							.ToList();

    // Compare it with the standard EF fluent API only
	var selectedBooks = db.Books
	                        .Where(b => b.Title.Contains(search))
							.OrderByDescending(b => b.Year)
							.ThenBy(b => b.Title)
							.Skip((pageNumber-1) * pageSize)
							.Take(pageSize)
							.ToList();

Well, it seems we didn't gain very much with IDNameObjects API in this example, but don't forget about the "search is null or empty" case.

#### Special filtering

- WhereID(bookID) - analog of Where(b => b.ID == bookID)

- WhereIDsIn(ids) - that's a DefaultWhere(ids) actually

- WhereIDsNotIn(ids) - I hope the name speaks for itself

- WhereNameContains(search) - the same as DefaultWhere(search)

- WhereNameStartsWith(search) - this _could be_ the same as DefaultWhere(search), but you have to set [IDNameObject(NameSearchType = "StartsWith")] attribute for this

Well, maybe you won't use these much, but note that you can use them along with the standard fluent API:

    var selectedBooks = db.Books.WhereIDsNotIn(ids).WhereNameContains(search).OrderBy(b => b.Title).ToList();

### Optimization

In some cases, you don't really need to fetch from the DB all the model's data. Let's remember our select lists: what they really need to work properly is ID+Name pairs. Well, that's not exactly true: when your model consists of composite [Name] property you ought to retrieve more data. But in simple cases, it is.

Actually, methods like ToSelect2PageResult, ToSelectList and ToMultiSelectList automatically retrieve only required minimum (i.e. ID+Name) if it is possible, so here you don't have to bother. But if you want, you could also use methods like AsSimpleINOs() and QuickSimpleINOQuery(). They work with SimpleIDNameObject class that consists of only two properties: ID and Name. With their help, you could optimize DB load a little.

### And more

There are a couple of useful helper methods, like counting items by default filters or creating list of ids from a list of items. See API subfolders for details.

### Going async

Almost forgot: for many of previously mentioned methods there is an asyncronous "cousin", just in case. Use them as usual:

    var select2result = await db.Books.ToSelect2PageResultAsync(search, pageNumber, pageSize);

## Where it could be used additionally

I think it could work pretty well with Windows lists controls, but I don't know of any that support partial data retrieving. If there are, I am sure I could write an additional IDNameObjects Windows API.

I also succesfully used it with <a href="https://github.com/martijnboland/MvcPaging">Martijn Boland's Mvc.Paging tool</a> (this paginator doesn't require loading all the data from the DB, which is good).

Here is an example (let's do it asynchronous):

    private async Task<IPagedList<Book>> GetCurPageListAsync(string search, int pageNumber, int pageSize)
    {
	    // total number of books filtered by search substring
        int totalCnt = await db.Books.DefaultCountAsync(search);

		// list of filtered books on current page
        var curPageList = await db.Books.QuickListAsync(search, pageNumber, pageSize);

        // here we call an Mvc.Paging method that creates a paged list from a list of books
		// Note that in Mvc.Paging pages are numbered from 0
        return curPageList.ToPagedList(pageNumber-1, pageSize, totalCnt);
    }

## Installation

You'll need to additionally install Entity Framework package and, for the Web part of the API, ASP.NET MVC.

This should work well on .NET 4.6 and with MSSQL Server. Haven't tested it on .NET Core yet.