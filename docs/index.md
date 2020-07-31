---
layout: default
---

# Microsoft Health Architectures 
Health Architectures is a collection of reference architectures and, when appropriate, implementations. They illustrate end-to-end best practices for using the Azure API for FHIR and related technologies.  As you look through this site, you will see some typical 'hello world' examples as well as more complex solutions. We conduct peer reviews to bring you the best practices for using the Microsoft Health technologies. 

<br>

<h2>Latest Posts</h2>

<ul>
  {% for post in site.posts %}
    <li>
      <h3><a href="{{ post.url | absolute_url }}">{{ post.excerpt }} </a></h3>
    </li>
  {% endfor %}
</ul>



We invite you to ask questions, make suggestions and share use cases which we might consider for future reference architectures or implementations.
