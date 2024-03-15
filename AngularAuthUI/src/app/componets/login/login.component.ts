import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  type:string = "password";
  isText:boolean=false;
  eyeIcon: string = "fa fa-eye-slash";
  constructor(){

  }
  
  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }
  hideShowPass(){
this.isText = !this.isText;
this.isText ? this.eyeIcon = "fa-eye" : this.eyeIcon ="fa-eye-slash";
this.isText ? this.type = "Text" :this.type = "password"
  }
}
