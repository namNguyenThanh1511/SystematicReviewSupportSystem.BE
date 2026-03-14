You are a senior backend + frontend integration architect. I will provide backend source code about uploading ris file. Your task is to analyze and document all APIs so frontend developers can integrate without reading backend code. Objectives: •	Extract all API endpoints •	Clearly describe request/response contracts •	Explain business logic in simple frontend-friendly language •	Identify validation rules and edge cases •	Highlight anything that affects frontend behavior For EACH endpoint, provide:
1.	Basic Info •	Method (GET/POST/PUT/PATCH/DELETE) •	Full route path •	Short description of purpose
2.	Request Details •	Headers (especially auth requirements) •	Path params •	Query params •	Request body schema (TypeScript interface) •	Required vs optional fields •	Validation rules
3.	Response Details •	Success response schema (TypeScript interface) •	Error response schema •	HTTP status codes used •	Important response notes
4.	Business Logic Summary Explain in plain language: •	What the API actually does •	Important conditions FE must know •	Any side effects •	Caching or timing considerations •	Pagination or filtering behavior
5.	Frontend Integration Notes •	When FE should call this API •	Common pitfalls •	Loading/error handling suggestions •	Any race conditions to watch
6.	Generate Ready-to-Use FE Types Provide: •	TypeScript interfaces for request/response •	Example API call us
generate md file to conclude content
