import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import ValidateForm from 'src/app/helpers/validateform';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent implements OnInit{

  type:string = "password";
  isText:boolean=false;
  eyeIcon: string = "fa fa-eye-slash";
  signUpForm!:FormGroup;
  constructor(private fb:FormBuilder , private auth:AuthService){}
  
  ngOnInit(): void {
   this.signUpForm = this.fb.group({
    firstName:['',Validators.required],
    lastName : ['',Validators.required],
    userName:['', Validators.required],
    email:['', Validators.required],
    password:['', Validators.required],
   })
  }
  
  hideShowPass(){
    this.isText = !this.isText;
    this.isText ? this.eyeIcon = "fa-eye" : this.eyeIcon ="fa-eye-slash";
    this.isText ? this.type = "Text" :this.type = "password"
  }

  onSignup(){
    if(this.signUpForm.valid)
    {
       this.auth.SignUp(this.signUpForm.value).subscribe({
         next:(res=>{
           alert(res.message)
         }),
       error:(err)=>{
          alert(err?.error.message)
        }
       })
      console.log(this.signUpForm.value);
    }else
    {
      ValidateForm.validateAllFormFileds(this.signUpForm)
      alert("Please check the form for any error");
    }

  }

  
}
