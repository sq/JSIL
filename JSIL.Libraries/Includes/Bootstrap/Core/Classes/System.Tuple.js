JSIL.ImplementExternals(
  "System.Tuple`1", function ($) {
  	var mscorlib = JSIL.GetCorlib();

	function initFields(self) {
		self.m_Item1 = null;
	}

		$.Method({ Static: false, Public: true }, ".ctor",
			(new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T1", "System.Tuple`1")], [])),
			function _ctor(item1) {
				initFields(this);

				this.m_Item1 = item1;
			});

		$.Method({ Static: false, Public: true }, "get_Item1",
			(new JSIL.MethodSignature(new JSIL.GenericParameter("T1", "System.Tuple`1"), [], [])),
			function get_Item1() {
				return this.m_Item1;
			});
	}
);

JSIL.ImplementExternals(
  "System.Tuple`2", function ($) {
  	function initFields(self) {
  		self.m_Item1 = null;
  		self.m_Item2 = null;
  	}

  	$.Method({ Static: false, Public: true }, ".ctor",
		(new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T1", "System.Tuple`2"), new JSIL.GenericParameter("T2", "System.Tuple`2")], [])),
		function _ctor(item1, item2) {
			initFields(this);

			this.m_Item1 = item1;
			this.m_Item2 = item2;
		});

  	$.Method({ Static: false, Public: true }, "get_Item1",
		(new JSIL.MethodSignature(new JSIL.GenericParameter("T1", "System.Tuple`2"), [], [])),
		function get_Item1() {
			return this.m_Item1;
		});

  	$.Method({ Static: false, Public: true }, "get_Item2",
		(new JSIL.MethodSignature(new JSIL.GenericParameter("T2", "System.Tuple`2"), [], [])),
		function get_Item2() {
			return this.m_Item2;
		});
  }
);

JSIL.ImplementExternals(
  "System.Tuple`3", function ($) {
  	function initFields(self) {
  		self.m_Item1 = null;
  		self.m_Item2 = null;
  		self.m_Item3 = null;
  	}

  	$.Method({ Static: false, Public: true }, ".ctor",
		(new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T1", "System.Tuple`3"), new JSIL.GenericParameter("T2", "System.Tuple`3"), new JSIL.GenericParameter("T3", "System.Tuple`3")], [])),
		function _ctor(item1, item2, item3) {
			initFields(this);

			this.m_Item1 = item1;
			this.m_Item2 = item2;
			this.m_Item3 = item3;
		});

  	$.Method({ Static: false, Public: true }, "get_Item1",
		(new JSIL.MethodSignature(new JSIL.GenericParameter("T1", "System.Tuple`3"), [], [])),
		function get_Item1() {
			return this.m_Item1;
		});

  	$.Method({ Static: false, Public: true }, "get_Item2",
		(new JSIL.MethodSignature(new JSIL.GenericParameter("T2", "System.Tuple`3"), [], [])),
		function get_Item2() {
			return this.m_Item2;
		});

  	$.Method({ Static: false, Public: true }, "get_Item3",
		(new JSIL.MethodSignature(new JSIL.GenericParameter("T3", "System.Tuple`3"), [], [])),
		function get_Item3() {
			return this.m_Item3;
		});
  }
);

