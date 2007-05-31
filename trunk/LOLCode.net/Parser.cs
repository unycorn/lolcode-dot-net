using System.Collections.Generic;
using System.Reflection.Emit;

using System;

namespace notdot.LOLCode {



internal partial class Parser {
	const int _EOF = 0;
	const int _ident = 1;
	const int _intCon = 2;
	const int _realCon = 3;
	const int _stringCon = 4;
	const int _eos = 5;
	const int _in = 6;
	const int maxT = 45;
	const int _comment = 46;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public string filename;
public Program program;

private bool IsArrayIndex() {
	return scanner.Peek().kind == _in;
}

private CodePragma GetPragma(Token tok) {
	return new CodePragma(filename, tok.line, tok.col);
}

void Error (string s) {
  if (errDist >= minErrDist) errors.SemErr(la.line, la.col, s);
  errDist = 0;
}



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }
				if (la.kind == 46) {
				}

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}
	
	bool WeakSeparator (int n, int syFol, int repFol) {
		bool[] s = new bool[maxT+1];
		if (la.kind == n) { Get(); return true; }
		else if (StartOf(repFol)) return false;
		else {
			for (int i=0; i <= maxT; i++) {
				s[i] = set[syFol, i] || set[repFol, i] || set[0, i];
			}
			SynErr(n);
			while (!s[la.kind]) Get();
			return StartOf(syFol);
		}
	}
	
	void LOLCode() {
		program = new Program(GetPragma(la)); 
		Expect(7);
		while (la.kind == 5) {
			Get();
		}
		Statements(program.statements);
		Expect(8);
		while (la.kind == 5) {
			Get();
		}
	}

	void Statements(List<Statement>statements) {
		Statement stat; 
		while (StartOf(1)) {
			if (la.kind == 9) {
				CanHasStatement();
			} else {
				Statement(out stat);
				statements.Add(stat); 
			}
			while (la.kind == 5) {
				Get();
			}
		}
	}

	void CanHasStatement() {
		Expect(9);
		Expect(10);
		if (la.kind == 4) {
			Get();
		} else if (la.kind == 1) {
			Get();
		} else SynErr(46);
		Expect(11);
	}

	void Statement(out Statement stat) {
		stat = null; 
		switch (la.kind) {
		case 12: {
			GimmehStatement(out stat);
			break;
		}
		case 17: {
			IHasAStatement(out stat);
			break;
		}
		case 19: {
			LoopStatement(out stat);
			break;
		}
		case 16: {
			GTFOStatement(out stat);
			break;
		}
		case 22: case 23: case 24: case 25: {
			BinaryOpStatement(out stat);
			break;
		}
		case 27: {
			IzStatement(out stat);
			break;
		}
		case 1: {
			FuncCallStatement(out stat);
			break;
		}
		case 30: {
			AssignmentStatement(out stat);
			break;
		}
		default: SynErr(47); break;
		}
	}

	void GimmehStatement(out Statement stat) {
		InputStatement ins = new InputStatement(GetPragma(la)); stat = ins; 
		Expect(12);
		if (la.kind == 13 || la.kind == 14 || la.kind == 15) {
			if (la.kind == 13) {
				Get();
			} else if (la.kind == 14) {
				Get();
				ins.amount = IOAmount.Word; 
			} else {
				Get();
				ins.amount = IOAmount.Letter; 
			}
		}
		LValue(out ins.dest);
	}

	void IHasAStatement(out Statement stat) {
		VariableDeclarationStatement vds = new VariableDeclarationStatement(GetPragma(la)); stat = vds; 
		Expect(17);
		Expect(10);
		Expect(18);
		Expect(1);
		vds.name = t.val; 
	}

	void LoopStatement(out Statement stat) {
		LoopStatement ls = new LoopStatement(GetPragma(la)); stat = ls; 
		Expect(19);
		Expect(6);
		Expect(20);
		Expect(1);
		ls.name = t.val; 
		while (la.kind == 5) {
			Get();
		}
		Statements(ls.statements);
		Expect(21);
	}

	void GTFOStatement(out Statement stat) {
		stat = new BreakStatement(GetPragma(la)); 
		Expect(16);
	}

	void BinaryOpStatement(out Statement stat) {
		BinaryOpStatement bos = new BinaryOpStatement(GetPragma(la)); bos.amount = new PrimitiveExpression(GetPragma(la), 1); stat = bos; 
		if (la.kind == 22) {
			Get();
			bos.op = OpCodes.Add; 
		} else if (la.kind == 23) {
			Get();
			bos.op = OpCodes.Sub; 
		} else if (la.kind == 24) {
			Get();
			bos.op = OpCodes.Mul; 
		} else if (la.kind == 25) {
			Get();
			bos.op = OpCodes.Div; 
		} else SynErr(48);
		LValue(out bos.lval);
		Expect(26);
		if (la.kind == 1 || la.kind == 2 || la.kind == 4) {
			Expression(out bos.amount);
		}
	}

	void IzStatement(out Statement stat) {
		ConditionalStatement cs = new ConditionalStatement(GetPragma(la)); stat = cs; Statement st; 
		Expect(27);
		Expression(out cs.condition);
		Expect(11);
		while (la.kind == 5) {
			Get();
		}
		if (la.kind == 28) {
			Get();
			while (la.kind == 5) {
				Get();
			}
			Statements(cs.trueStatements);
		}
		if (la.kind == 29) {
			Get();
			while (la.kind == 5) {
				Get();
			}
			Statements(cs.falseStatements);
		}
		Expect(21);
	}

	void FuncCallStatement(out Statement stat) {
		FuncCallStatement fcs = new FuncCallStatement(GetPragma(la)); stat = fcs; Expression parm; 
		Expect(1);
		fcs.name = t.val; 
		if (la.kind == 1 || la.kind == 2 || la.kind == 4) {
			Unary(out parm);
			fcs.arguments.Add(parm); 
			while (la.kind == 1 || la.kind == 2 || la.kind == 4) {
				Unary(out parm);
				fcs.arguments.Add(parm); 
			}
		}
	}

	void AssignmentStatement(out Statement stat) {
		AssignmentStatement ass = new AssignmentStatement(GetPragma(la)); stat = ass; 
		Expect(30);
		LValue(out ass.lval);
		Expect(31);
		Expression(out ass.rval);
	}

	void LValue(out LValue lv) {
		lv = null; 
		if (!IsArrayIndex()) {
			Expect(1);
			lv = new VariableLValue(GetPragma(la), t.val); 
		} else if (la.kind == 1 || la.kind == 2) {
			ArrayIndex(out lv);
		} else SynErr(49);
	}

	void Expression(out Expression exp) {
		Expression left; 
		Unary(out left);
		AndExpression(out exp, left);
	}

	void Unary(out Expression exp) {
		exp = null; 
		if (la.kind == _intCon && !IsArrayIndex()) {
			Expect(2);
			exp = new PrimitiveExpression(GetPragma(la), int.Parse(t.val)); 
		} else if (la.kind == 1 || la.kind == 2) {
			LValueExpression lve = new LValueExpression(GetPragma(la)); exp = lve; 
			LValue(out lve.lval);
		} else if (la.kind == 4) {
			Get();
			exp = new PrimitiveExpression(GetPragma(la), t.val.Substring(1, t.val.Length - 2)); 
		} else SynErr(50);
	}

	void AndExpression(out Expression exp, Expression left) {
		XorExpression(out exp, left);
		while (la.kind == 32) {
			Get();
			IntegerBinaryExpression ibs = new IntegerBinaryExpression(GetPragma(la)); ibs.op = OpCodes.And; ibs.left = exp; exp = ibs; 
			Unary(out ibs.right);
			XorExpression(out ibs.right, ibs.right);
		}
	}

	void XorExpression(out Expression exp, Expression left) {
		OrExpression(out exp, left);
		while (la.kind == 33) {
			Get();
			IntegerBinaryExpression ibs = new IntegerBinaryExpression(GetPragma(la)); ibs.op = OpCodes.Xor; ibs.left = exp; exp = ibs; 
			Unary(out ibs.right);
			OrExpression(out ibs.right, ibs.right);
		}
	}

	void OrExpression(out Expression exp, Expression left) {
		ComparisonExpression(out exp, left);
		while (la.kind == 34) {
			Get();
			IntegerBinaryExpression ibs = new IntegerBinaryExpression(GetPragma(la)); ibs.op = OpCodes.Or; ibs.left = exp; exp = ibs; 
			Unary(out ibs.right);
			ComparisonExpression(out ibs.right, ibs.right);
		}
	}

	void ComparisonExpression(out Expression exp, Expression left) {
		ArithmeticExpression(out exp, left);
		while (StartOf(2)) {
			IntegerBinaryExpression ibs = new IntegerBinaryExpression(GetPragma(la)); ibs.left = exp; exp = ibs; 
			if (la.kind == 35) {
				Get();
				ibs.negate = true; 
			}
			if (la.kind == 36) {
				Get();
				ibs.op = OpCodes.Cgt; 
				if (la.kind == 37) {
					Get();
				}
			} else if (la.kind == 38) {
				Get();
				ibs.op = OpCodes.Clt; 
				if (la.kind == 37) {
					Get();
				}
			} else if (la.kind == 39) {
				Get();
				ibs.op = OpCodes.Ceq; 
			} else SynErr(51);
			Unary(out ibs.right);
			ArithmeticExpression(out ibs.right, ibs.right);
		}
	}

	void ArithmeticExpression(out Expression exp, Expression left) {
		MultiplicationExpression(out exp, left);
		while (la.kind == 40 || la.kind == 41) {
			IntegerBinaryExpression ibs = new IntegerBinaryExpression(GetPragma(la)); ibs.left = exp; exp = ibs; 
			if (la.kind == 40) {
				Get();
				ibs.op = OpCodes.Add; 
			} else {
				Get();
				ibs.op = OpCodes.Sub; 
			}
			Unary(out ibs.right);
			MultiplicationExpression(out ibs.right, ibs.right);
		}
	}

	void MultiplicationExpression(out Expression exp, Expression left) {
		exp = left; 
		while (la.kind == 42 || la.kind == 43) {
			IntegerBinaryExpression ibs = new IntegerBinaryExpression(GetPragma(la)); ibs.left = exp; exp = ibs; 
			if (la.kind == 42) {
				Get();
				ibs.op = OpCodes.Mul; 
			} else {
				Get();
				ibs.op = OpCodes.Div; 
			}
			Unary(out ibs.right);
		}
	}

	void ArrayIndex(out LValue lv) {
		ArrayIndexLValue alv = new ArrayIndexLValue(GetPragma(la)); lv = alv; 
		if (la.kind == 1) {
			Get();
			alv.index = new LValueExpression(GetPragma(la), new VariableLValue(GetPragma(la), t.val)); 
		} else if (la.kind == 2) {
			Get();
			alv.index = new PrimitiveExpression(GetPragma(la), int.Parse(t.val)); 
		} else SynErr(52);
		Expect(6);
		Expect(44);
		LValue(out alv.lval);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		LOLCode();

    Expect(0);
	}
	
	bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,x, x,x,x,x, x,T,x,x, T,x,x,x, T,T,x,T, x,x,T,T, T,T,x,T, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, T,x,T,T, x,x,x,x, x,x,x}

	};
} // end Parser


public class Errors : System.Collections.Generic.List<string> {
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text
  
	public void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "intCon expected"; break;
			case 3: s = "realCon expected"; break;
			case 4: s = "stringCon expected"; break;
			case 5: s = "eos expected"; break;
			case 6: s = "in expected"; break;
			case 7: s = "\"hai\" expected"; break;
			case 8: s = "\"kthxbye\" expected"; break;
			case 9: s = "\"can\" expected"; break;
			case 10: s = "\"has\" expected"; break;
			case 11: s = "\"?\" expected"; break;
			case 12: s = "\"gimmeh\" expected"; break;
			case 13: s = "\"line\" expected"; break;
			case 14: s = "\"word\" expected"; break;
			case 15: s = "\"lettar\" expected"; break;
			case 16: s = "\"gtfo\" expected"; break;
			case 17: s = "\"i\" expected"; break;
			case 18: s = "\"a\" expected"; break;
			case 19: s = "\"im\" expected"; break;
			case 20: s = "\"yr\" expected"; break;
			case 21: s = "\"kthx\" expected"; break;
			case 22: s = "\"upz\" expected"; break;
			case 23: s = "\"nerfz\" expected"; break;
			case 24: s = "\"tiemzd\" expected"; break;
			case 25: s = "\"ovarz\" expected"; break;
			case 26: s = "\"!!\" expected"; break;
			case 27: s = "\"iz\" expected"; break;
			case 28: s = "\"yarly\" expected"; break;
			case 29: s = "\"nowai\" expected"; break;
			case 30: s = "\"lol\" expected"; break;
			case 31: s = "\"r\" expected"; break;
			case 32: s = "\"and\" expected"; break;
			case 33: s = "\"xor\" expected"; break;
			case 34: s = "\"or\" expected"; break;
			case 35: s = "\"not\" expected"; break;
			case 36: s = "\"bigr\" expected"; break;
			case 37: s = "\"than\" expected"; break;
			case 38: s = "\"smalr\" expected"; break;
			case 39: s = "\"liek\" expected"; break;
			case 40: s = "\"up\" expected"; break;
			case 41: s = "\"nerf\" expected"; break;
			case 42: s = "\"tiemz\" expected"; break;
			case 43: s = "\"ovar\" expected"; break;
			case 44: s = "\"mah\" expected"; break;
			case 45: s = "??? expected"; break;
			case 46: s = "invalid CanHasStatement"; break;
			case 47: s = "invalid Statement"; break;
			case 48: s = "invalid BinaryOpStatement"; break;
			case 49: s = "invalid LValue"; break;
			case 50: s = "invalid Unary"; break;
			case 51: s = "invalid ComparisonExpression"; break;
			case 52: s = "invalid ArrayIndex"; break;

			default: s = "error " + n; break;
		}
		base.Add(string.Format(errMsgFormat, line, col, s));
	}

	public void SemErr (int line, int col, string s) {
		base.Add(string.Format(errMsgFormat, line, col, s));
	}
	
	public void SemErr (string s) {
		base.Add(s);
	}
	
	public void Warning (int line, int col, string s) {
		base.Add(string.Format(errMsgFormat, line, col, s));
	}
	
	public void Warning(string s) {
		base.Add(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}

}