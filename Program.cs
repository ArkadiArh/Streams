//*****************************************
/* Program learn working with streams.*/
//*****************************************
using System.IO;
using System.Xml;
using static System.Console;
using static System.Environment;
using static System.IO.Path;
using System.IO.Compression;

internal class Program
{
  // определение позывных пользователей  
    static string[] nameUsers = new string[] {
        "Пушкин", "Альберт", "Молодой", "Щегол",
        "Садовод", "Араннгел", "Ворон", "Вожак"};

  static void DoWithText()
  {
    StreamWriter? txt = null;

    try
    {
      // обозначение файла для записи в него потока
      string txtFile = Combine(CurrentDirectory, "streams.txt");

      // создание текстового файла
      txt = File.CreateText(txtFile);

      // перечисление строк с записью каждой из них 
      // в поток. Запись выполняется в отдельной строке
      foreach (string name in nameUsers)
      {
        txt.WriteLine(name);
      }
      txt.Close(); // освобождение ресурсов потока

      // вывод содержания файла на экран
      WriteLine("Файл {0} содержит {1:N0} bytes.", 
        arg0: txtFile,
        arg1: new FileInfo(txtFile).Length);
      WriteLine("Содержимое файла:");
      WriteLine(File.ReadAllText(txtFile));
    }
    catch(Exception exceptionMistake)
    {
      // В случае отсутствия указанного пути на экран выводится исключение
      WriteLine($"{exceptionMistake.GetType()} says {exceptionMistake.Message}");
    }

    finally
    {
      if(txt != null)
      {
        txt.Dispose();
        WriteLine("Ресурсы, выделенные для выполнения работ с " +
        "потоком записи в файл stream.txt, успешно освобождены");
      }
    }
  }

  static void DoWithXml()
  {
    FileStream? xmlFileStream = null;
    XmlWriter? xml = null;

    try
    {
      // обозначение файла для записи в него потока
      string xmlFile = Combine(CurrentDirectory, "streams.xml");

      // содание файлового потока
      xmlFileStream = File.Create(xmlFile);

      // оборачивание файлового потока во вспомогательный объект
      // для записи XML и автоматическое добавление 
      // отступов для вложения элементов
      xml = XmlWriter.Create(xmlFileStream, new XmlWriterSettings { Indent = true });

      // запись обновления XML
      xml.WriteStartDocument();

      // запись корневого элемента
      xml.WriteStartElement("nameUsers");

      // перечисление строк с записью каждой из них в поток
      foreach (string name in nameUsers)
      {
        xml.WriteElementString("nameUsers", name);
      }

      // запись закрывающего корневого элемента
      xml.WriteEndElement();

      // закрытие вспомогательного объекта и потока
      xml.Close();
      xmlFileStream.Close();

      // вывод содержимого файла
      WriteLine("Файл {0} содержит {1:N0} bytes.",
      arg0: xmlFile,
      arg1: new FileInfo(xmlFile).Length);
      
      WriteLine("Содержимое файла:");
      WriteLine(File.ReadAllText(xmlFile));
    }

    catch(Exception exceptionMistake)
    {
      // если путь не существует, то выбрасывается исключение
      WriteLine($"{exceptionMistake.GetType()} сообщает {exceptionMistake.Message}");
    }

    finally
    {
      if(xml != null)
      {
        xml.Dispose();
        WriteLine("Ресурсы, выделенные для выполнения работ с потоком "
        +"для записи в формате Xml, успешно освобождены");
      }

      if (xmlFileStream != null)
      {
        xmlFileStream.Dispose();
        WriteLine("Ресурсы, выделенные для выполнения работ с потоком "+
        "записи в файл, успешно освобождены");
      }
    }
  }
  static void DoWithTextAndUseStream()
  {
    using (FileStream file2 = File.OpenWrite(Path.Combine(CurrentDirectory, "file2.txt")))
    {
      using (StreamWriter writer2 = new StreamWriter(file2))
      {
        try
        {
          writer2.WriteLine("Добро пожаловать  .Net Core!");
        }
        catch (Exception exceptionMistake)
        {
          WriteLine($"Исключение {exceptionMistake.GetType()} сообщило {exceptionMistake.Message} ");
        }
      } // автоматический вызов метода Dispose, если объект не равен null
    } // автоматический вызов метода Dispose, если объект не равен null
  }

  static void DoWithCompression()
  {
    // сжатие XML-вывода
    string gzipFilePath = Combine(CurrentDirectory, "stream.gzip");

    FileStream gzipFile = File.Create(gzipFilePath);

    using (GZipStream compressor = new GZipStream (gzipFile, CompressionMode.Compress))
    {
      using (XmlWriter xmlGzip = XmlWriter.Create(compressor))
      {
        xmlGzip.WriteStartDocument();
        xmlGzip.WriteStartElement("nameUsers");

        foreach (string name in nameUsers)
        {
          xmlGzip.WriteElementString("nameUsers", name);
        }
        // вызов метода WriteEndElement необязателен, 
        // поскольку, освобождаясь, XmlWriter
        // автоматически закрывает любые элементы
      }
    } // закрытие основного потока

    // выводит всё содержимое сжатого файла в консоль
    WriteLine("Файл {0} содержит {1:N0} bytes.", 
        gzipFilePath, new FileInfo(gzipFilePath).Length);

    WriteLine($"Содержимое после сжатия:");
    WriteLine(File.ReadAllText(gzipFilePath));

    // чтение сжатого файла
    WriteLine("Чтение XML файла:");
    gzipFile = File.Open(gzipFilePath, FileMode.Open); 
    
    using (GZipStream decompressor = new GZipStream(
      gzipFile, CompressionMode.Decompress))
    {
      using (XmlReader reader = XmlReader.Create(decompressor))
      {
        while (reader.Read()) // чтение сжатого файла
        {
          // проверяет, находимся ли мы на элементе с именем nameUsers
          if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "nameUsers"))
          {
            reader.Read(); // переход к тексту внутри элемента
            WriteLine($"{reader.Value}"); // чтение его значения
          }
        }
      }
    }
  }

  static void DoWitnAlgoritmBrotliCompression(bool useBrotli = true)
  {
    string fileExt = useBrotli ? "brotli" : "gzip";

    // сжатие XML- вывода
    string filePath = Combine(
      CurrentDirectory, $"streams.{fileExt}");
    
    FileStream file = File.Create(filePath);

    Stream compressor;
    if (useBrotli)
    {
      compressor = new BrotliStream(file, CompressionMode.Compress);
    }
    else
    {
      compressor = new GZipStream(file, CompressionMode.Compress);
    }

    using (compressor)
    {
      using (XmlWriter xml = XmlWriter.Create(compressor))
      {
        xml.WriteStartDocument();
        xml.WriteStartElement("nameUsers");
        foreach (string name in nameUsers)
        {
          xml.WriteElementString("nameUsers", name);
        }
      }
    } // закрытие основного потока

    // выводит все содержимое сжатого файла в консоль
    WriteLine("Файл {0} содержит {1:N0} bytes.", filePath, new FileInfo(filePath).Length);
    WriteLine("Содержимое файла:");
    WriteLine(File.ReadAllText(filePath));

    // чтение сжатого файла
    WriteLine("Чтение сжатого XML файла:");
    file = File.Open(filePath, FileMode.Open);

    Stream decompressor;
    if(useBrotli)
    {
      decompressor = new BrotliStream(file, CompressionMode.Decompress);
    }
    else
    {
      decompressor = new GZipStream(file, CompressionMode.Decompress);
    }

    using (decompressor)
    {
      using (XmlReader reader = XmlReader.Create(decompressor))
      {
        while (reader.Read())
        {
          // проверяет, находимся ли мы на элементе с именем nameUsers
          if((reader.NodeType == XmlNodeType.Element) && (reader.Name == "nameUsers"))
          {
            reader.Read(); // переходк тексту внутри элемента
            WriteLine($"{reader.Value}"); // чтение его значения
          }
        }
      }
    }
  }

  private static void Main(string[] args)
  {
    DoWithText();
    DoWithXml();
    DoWithTextAndUseStream();
    DoWithCompression();
    DoWitnAlgoritmBrotliCompression(useBrotli: true);
  }
}

