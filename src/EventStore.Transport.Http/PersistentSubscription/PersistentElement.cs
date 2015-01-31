using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using EventStore.Common.Utils;
using EventStore.Transport.Http.Atom;
using Newtonsoft.Json;

namespace EventStore.Transport.Http.PersistentSubscription
{
    public class PersistentElement : IXmlSerializable
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string Updated { get; set; }
        public string StreamId { get; set; }
        public PersonElement Author { get; set; }
        public bool HeadOfStream { get; set; }
        public string SelfUrl { get; set; }
        public string ETag { get; set; }

        public List<LinkElement> Links { get; set; }
        public List<EntryElement> Entries { get; set; }

        public PersistentElement()
        {
            Links = new List<LinkElement>();
            Entries = new List<EntryElement>();
        }

        public void SetTitle(string title)
        {
            Ensure.NotNull(title, "title");
            Title = title;
        }

        public void SetId(string id)
        {
            Ensure.NotNull(id, "id");
            Id = id;
        }

        public void SetUpdated(DateTime dateTime)
        {
            Updated = XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc);
        }

        public void SetAuthor(string name)
        {
            Ensure.NotNull(name, "name");
            Author = new PersonElement(name);
        }

        public void SetHeadOfStream(bool headOfStream)
        {
            this.HeadOfStream = headOfStream;
        }

        public void SetSelfUrl(string self)
        {
            this.SelfUrl = self;
        }

        public void SetETag(string etag)
        {
            this.ETag = etag;
        }

        public void AddLink(string relation, string uri, string contentType = null)
        {
            Ensure.NotNull(uri, "uri");
            Links.Add(new LinkElement(uri, relation, contentType));
        }

        public void AddEntry(EntryElement entry)
        {
            Ensure.NotNull(entry, "entry");
            Entries.Add(entry);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }
        //TODO: CLC Review these to see if they still apply
        public void WriteXml(XmlWriter writer)
        {
            if (string.IsNullOrEmpty(Title))
                Atom.ThrowHelper.ThrowSpecificationViolation("persistent:feed elements MUST contain exactly one persistent:title element.");
            if (string.IsNullOrEmpty(Id))
                Atom.ThrowHelper.ThrowSpecificationViolation("persistent:feed elements MUST contain exactly one persistent:id element.");
            if (string.IsNullOrEmpty(Updated))
                Atom.ThrowHelper.ThrowSpecificationViolation("persistent:feed elements MUST contain exactly one persistent:updated element.");
            if (Author == null)
                Atom.ThrowHelper.ThrowSpecificationViolation("persistent:feed elements MUST contain one or more persistent:author elements");
            if (Links.Count == 0)
                Atom.ThrowHelper.ThrowSpecificationViolation("persistent:feed elements SHOULD contain one persistent:link element with a "
                                                        + "rel attribute value of 'self'.This is the preferred URI for retrieving Persistent Feed Documents                                                                   representing this Atom feed.");

            writer.WriteStartElement("feed", PersistentSpecs.AtomV1Namespace);

            writer.WriteElementString("title", PersistentSpecs.AtomV1Namespace, Title);
            writer.WriteElementString("id", PersistentSpecs.AtomV1Namespace, Id);
            writer.WriteElementString("updated", PersistentSpecs.AtomV1Namespace, Updated);
            Author.WriteXml(writer);

            Links.ForEach(link => link.WriteXml(writer));
            Entries.ForEach(entry => entry.WriteXml(writer, usePrefix: false));

            writer.WriteEndElement();
        }
    }

    public class EntryElement : IXmlSerializable
    {
        private object _content;
        public string Title { get; set; }
        public string Id { get; set; }
        public string Updated { get; set; }
        public PersonElement Author { get; set; }
        public string Summary { get; set; }

        public object Content {
            get { return _content; } 
            set { throw new NotSupportedException(); } 
        }

        public List<LinkElement> Links { get; set; }

        public EntryElement()
        {
            Links = new List<LinkElement>();
        }

        public void SetTitle(string title)
        {
            Ensure.NotNull(title, "title");
            Title = title;
        }

        public void SetId(string id)
        {
            Ensure.NotNull(id, "id");
            Id = id;
        }

        public void SetUpdated(DateTime dateTime)
        {
            Updated = XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc);
        }

        public void SetAuthor(string name)
        {
            Ensure.NotNull(name, "name");
            Author = new PersonElement(name);
        }

        public void SetSummary(string summary)
        {
            Ensure.NotNull(summary, "summary");
            Summary = summary;
        }

        public void AddLink(string relation, string uri, string type = null)
        {
            Ensure.NotNull(uri, "uri");
            Links.Add(new LinkElement(uri, relation, type));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("entry");

            Title = reader.ReadElementString("title");
            Id = reader.ReadElementString("id");
            Updated = reader.ReadElementString("updated");
            Author.ReadXml(reader);
            Summary = reader.ReadElementString("summary");
            Links.ForEach(l => l.ReadXml(reader));

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, usePrefix: true);
        }

        public void WriteXml(XmlWriter writer, bool usePrefix)
        {
            if (string.IsNullOrEmpty(Title))
                 Atom.ThrowHelper.ThrowSpecificationViolation("persistent:entry elements MUST contain exactly one persistent:title element.");
            if (string.IsNullOrEmpty(Id))
                 Atom.ThrowHelper.ThrowSpecificationViolation("persistent:entry elements MUST contain exactly one persistent:id element.");
            if (string.IsNullOrEmpty(Updated))
                 Atom.ThrowHelper.ThrowSpecificationViolation("persistent:entry elements MUST contain exactly one persistent:updated element.");
            if (Author == null)
                 Atom.ThrowHelper.ThrowSpecificationViolation("persistent:entry elements MUST contain one or more persistent:author elements");
            if (string.IsNullOrEmpty(Summary))
                Atom.ThrowHelper.ThrowSpecificationViolation("persistent:entry elements MUST contain an persistent:summary element");

            if (usePrefix)
                writer.WriteStartElement("atom", "entry", PersistentSpecs.AtomV1Namespace);
            else
                writer.WriteStartElement("entry", PersistentSpecs.AtomV1Namespace);

            writer.WriteElementString("title", PersistentSpecs.AtomV1Namespace, Title);
            writer.WriteElementString("id", PersistentSpecs.AtomV1Namespace, Id);
            writer.WriteElementString("updated", PersistentSpecs.AtomV1Namespace, Updated);
            Author.WriteXml(writer);
            writer.WriteElementString("summary", PersistentSpecs.AtomV1Namespace, Summary);
            Links.ForEach(link => link.WriteXml(writer));
            if (Content != null)
            {
                var serializeObject = JsonConvert.SerializeObject(Content);
                var deserializeXmlNode = JsonConvert.DeserializeXmlNode(serializeObject, "content");
                writer.WriteStartElement("content", PersistentSpecs.AtomV1Namespace);
                writer.WriteAttributeString("type", ContentType.ApplicationXml);
                deserializeXmlNode.DocumentElement.WriteContentTo(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public void SetContent(object content)
        {
            _content = content;
        }
    }

    public class RichEntryElement : EntryElement
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public int EventNumber { get; set; }
        public string Data { get; set; }
        public string MetaData { get; set; }
        public string LinkMetaData { get; set; }

        public string StreamId { get; set; }

        public bool IsJson { get; set; }

        public bool IsMetaData { get; set; }
        public bool IsLinkMetaData { get; set; }

        public int PositionEventNumber { get; set; }

        public string PositionStreamId { get; set; }
    }

    public class LinkElement : IXmlSerializable
    {
        public string Uri { get; set; }
        public string Relation { get; set; }
        public string Type { get; set; }

        public LinkElement(string uri) : this(uri, null, null)
        {
        }

        public LinkElement(string uri, string relation):this(uri, relation, null)
        {
        }

        public LinkElement(string uri, string relation, string type)
        {
            Uri = uri;
            Relation = relation;
            Type = type;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("link");

            Uri = reader.GetAttribute("href");
            Relation = reader.GetAttribute("rel");
            Type = reader.GetAttribute("type");

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (string.IsNullOrEmpty(Uri))
                Atom.ThrowHelper.ThrowSpecificationViolation("persistent:link elements MUST have an href attribute, whose value MUST be a IRI reference");

            writer.WriteStartElement("link", PersistentSpecs.AtomV1Namespace);
            writer.WriteAttributeString("href", Uri);

            if (Relation != null)
                writer.WriteAttributeString("rel", Relation);
            if (Type != null)
                writer.WriteAttributeString("type", Type);

            writer.WriteEndElement();
        }
    }

    public class PersonElement : IXmlSerializable
    {
        public string Name { get; set; }

        public PersonElement(string name)
        {
            Name = name;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("author");
            Name = reader.ReadElementString("name");
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (string.IsNullOrEmpty(Name))
                Atom.ThrowHelper.ThrowSpecificationViolation("Person constructs MUST contain exactly one 'persistent:name' element.");

            writer.WriteStartElement("author", PersistentSpecs.AtomV1Namespace);
            writer.WriteElementString("name", PersistentSpecs.AtomV1Namespace, Name);
            writer.WriteEndElement();
        }
    }
}
