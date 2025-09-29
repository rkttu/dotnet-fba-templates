// React를 전역 객체로 사용
declare var React: any;
declare var ReactDOM: any;

interface Person {
   firstName: string;
   lastName: string;
}

class Student {
   fullName: string;
   constructor(public firstName: string, public middleInitial: string, public lastName: string) {
      this.fullName = firstName + " " + middleInitial + " " + lastName;
   }
}

function greeter(person: Person) {
   return "Hello, " + person.firstName + " " + person.lastName;
}

const TSButton = () => {
   const user = new Student("Fred", "M.", "Smith");
   const [apiMessage, setApiMessage] = React.useState("");
   const [isLoading, setIsLoading] = React.useState(false);

   const handleClick = () => {
      alert(greeter(user));
   };

   const handleApiCall = async () => {
      setIsLoading(true);
      try {
         const response = await fetch('/api/hello');
         if (response.ok) {
            const data = await response.text();
            setApiMessage(data);
         } else {
            setApiMessage(`Error: ${response.status} ${response.statusText}`);
         }
      } catch (error) {
         setApiMessage(`Error: ${error instanceof Error ? error.message : 'Unknown error'}`);
      } finally {
         setIsLoading(false);
      }
   };

   return React.createElement("div", null,
      React.createElement("h2", null, "TypeScript + React Example"),
      React.createElement("button", { onClick: handleClick },
         "Click me to greet " + user.fullName
      ),
      React.createElement("p", null, "Greeting: " + greeter(user)),
      React.createElement("hr", null),
      React.createElement("h3", null, "API Test"),
      React.createElement("button", { 
         onClick: handleApiCall, 
         disabled: isLoading 
      }, isLoading ? "Loading..." : "Call /api/hello API"),
      React.createElement("p", null, "API Response: ", apiMessage)
   );
};

// DOM에 렌더링
window.addEventListener('DOMContentLoaded', () => {
   const container = document.getElementById('ts-example');
   if (container) {
      ReactDOM.render(React.createElement(TSButton), container);
   }
});
